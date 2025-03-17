using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Outputs;
using OpenUGD.ECS.Engine.Systems;
using OpenUGD.ECS.Engine.Utils;

namespace OpenUGD.ECS.Engine
{
    public abstract class ECSEngine : IECSEngine
    {
        public const int Version = 1;

        public abstract EngineEnvironment Environment { get; }
        public abstract int Tick { get; }
        public abstract IEngineInputs Inputs { get; }
        public abstract IEngineOutputs Outputs { get; }
        public abstract IEngineState State { get; }
        public abstract EngineSnapshot Snapshot();
        public abstract int FastForward(int tick);
        public abstract bool Backward();
        public abstract void StartOver(int randomSeed = -1);
        public abstract int GoTo(int tick);
        public abstract void Dispose();
    }

    public abstract class Engine<TWorld> : ECSEngine where TWorld : World
    {
        public abstract TWorld? World { get; }

        protected abstract void OnInitialized();
        protected abstract TWorld CreateWorld(int seed);
        protected abstract ISystem<TWorld>[] CreateSystems();
    }

    public abstract class Engine<TWorld, TConfiguration> : Engine<TWorld>, IECSEngine, IEngineState
        where TWorld : World
        where TConfiguration : EngineConfiguration
    {
        private readonly Queue<Input> _playedInputs;
        private readonly EngineInputs<TWorld> _inputs;
        private readonly EngineOutputs _outputs;
        private readonly EngineSystems<TWorld> _systems;
        private readonly Context _context;

        private int _seed;
        private int _tick;
        private TWorld? _world;

        class Context : IEngineContext
        {
            public int Tick;
            public int Seed { get; set; }
            public EngineOutputs Outputs;

            public T Enqueue<T>() where T : Output, new() => Outputs.Enqueue<T>(Tick);
        }

        protected Engine(
            TConfiguration configuration,
            Action<IAddCommand<TWorld>> options,
            Action<JsonSerializerSettings> settings = null
        )
            : this(configuration, CommandOptions(options), settings)
        {
        }

        protected Engine(
            TConfiguration configuration,
            InputCommands<TWorld> inputCommands,
            Action<JsonSerializerSettings> settings = null
        )
        {
            if (configuration.Tick < 0)
            {
                throw new ArgumentException("tick must be >= 0");
            }

            Serializer = new Serializer(settings);

            Configuration = configuration;

            _seed = configuration.RandomSeed;
            _playedInputs = new Queue<Input>();
            _outputs = new EngineOutputs();
            _inputs = new EngineInputs<TWorld>(this, inputCommands.Build(), Serializer);
            _systems = new EngineSystems<TWorld>(CreateSystems);
            _context = new Context
            {
                Outputs = _outputs,
            };

            InitializeState(_seed);
            InitializeInputs(configuration);

            var nextTick = Math.Max(configuration.Tick, 0);
            _context.Tick = _tick = nextTick;

            InitializeSystems();

            FastForward(nextTick);
        }

        public Serializer Serializer { get; }
        public TConfiguration Configuration { get; }
        public int Seed => _seed;
        public sealed override EngineEnvironment Environment => Configuration.Environment;
        public sealed override IEngineOutputs Outputs => _outputs;
        public sealed override IEngineState State => this;
        public sealed override IEngineInputs Inputs => _inputs;
        public sealed override int Tick => _tick;
        public bool IsExited => ExitState != ExitReason.None;
        public ExitReason ExitState { get; private set; }
        public sealed override TWorld? World => _world;

        public override EngineSnapshot Snapshot()
        {
            var snapshot = new EngineSnapshot(
                configuration: Configuration,
                inputs: _playedInputs.ToArray(),
                randomSeed: _seed,
                tick: Tick,
                serializer: Serializer
            );
            return snapshot;
        }

        public override int GoTo(int tick)
        {
            if (tick > Tick)
            {
                FastForward(tick);
            }
            else if (tick < Tick)
            {
                if (tick >= Math.Max(1, Configuration.Tick))
                {
                    StartOverInternal(-1);

                    while (Outputs.Count != 0)
                    {
                        Outputs.ReleaseToPool(Outputs.Dequeue());
                    }

                    if (Environment.IsGenerateOutputs())
                    {
                        _outputs.Enqueue<StartOverOutput>(0);
                    }

                    FastForward(tick);

                    if (!Configuration.Environment.IsSnapshot())
                    {
                        while (_inputs.Count != 0)
                        {
                            _inputs.Dequeue();
                        }
                    }

                    _outputs.ReturnToPoolAll();

                    if (Environment.IsGenerateOutputs())
                    {
                        _outputs.Enqueue<StartOverOutput>(tick);
                    }

                    OnInitialized();
                }
            }

            return Tick;
        }

        public sealed override int FastForward(int tick)
        {
            if (IsExited)
            {
                return Tick;
            }

            var finish = false;
            if (tick >= Configuration.MaxTicks && Environment.IsDebug())
            {
                tick = Configuration.MaxTicks;
                finish = true;
            }

            var currentTick = Tick;
            while (++currentTick <= tick && !IsExited)
            {
                SetTick(currentTick);

                Input input;
                while (!IsExited && _inputs.Count != 0 && (input = _inputs.Peek()).Tick <= currentTick)
                {
                    _playedInputs.Enqueue(_inputs.Dequeue());
                    _inputs.Execute(input);
                }

                if (!IsExited)
                {
                    _context.Tick = currentTick;
                    _systems.Tick(World!, currentTick, _context);
                }
            }

            if (!IsExited)
            {
                if (finish)
                {
                    Exit(ExitReason.MaxTick);
                }
            }

            return Tick;
        }

        public override bool Backward()
        {
            var currentTick = Tick;
            var newTick = GoTo(currentTick - 1);
            return newTick != currentTick;
        }

        public override void StartOver(int randomSeed = -1)
        {
            StartOverInternal(randomSeed);

            _inputs.Clear();

            if (Configuration.Inputs != null)
            {
                foreach (var action in Configuration.Inputs)
                {
                    Inputs.AddInput(action);
                }
            }

            while (Outputs.Count != 0)
            {
                Outputs.ReleaseToPool(Outputs.Dequeue());
            }

            if (Environment.IsGenerateOutputs())
            {
                _outputs.Enqueue<StartOverOutput>(0);
            }

            OnInitialized();

            var nextTick = Math.Max(Configuration.Tick, 0);
            FastForward(nextTick);
        }

        public void Continue() => ExitState = ExitReason.None;

        public void Exit(ExitReason reason) => ExitState = reason;


        public override void Dispose()
        {
            if (_world != null)
            {
                foreach (var subWorld in _world)
                {
                    subWorld.DeleteAll();
                }

                _world.Pool.Clear();
            }
        }

        private void SetTick(int currentTick) => _tick = currentTick;

        private void StartOverInternal(int randomSeed)
        {
            _inputs.Clear();

            InitializeState(randomSeed);

            _context.Tick = Tick;
            _systems.Initialize(World!, _context);

            if (!Environment.IsSnapshot())
            {
                foreach (var action in _playedInputs)
                {
                    _inputs.Enqueue(action);
                }
            }

            if (Environment.IsSnapshot() && Configuration.Inputs != null)
            {
                foreach (var action in Configuration.Inputs)
                {
                    if (!_inputs.Contains(action.GetType(), action.Tick))
                    {
                        _inputs.AddInput(action);
                    }
                }
            }

            _playedInputs.Clear();
        }

        private void InitializeSystems()
        {
            _systems.Initialize(_world!, _context);
            OnInitialized();
        }

        private void InitializeInputs(TConfiguration configuration)
        {
            if (configuration.Inputs != null)
            {
                foreach (var action in configuration.Inputs)
                {
                    Inputs.AddInput(action);
                }
            }
        }

        private void InitializeState(int randomSeed)
        {
            ExitState = ExitReason.None;
            _tick = 0;

            var seed = randomSeed;

            if (_world != null && randomSeed < 0)
            {
                seed = _seed;
            }

            _seed = seed;
            _context.Seed = seed;
            _world = CreateWorld(seed);
        }

        private static InputCommands<TWorld> CommandOptions(Action<IAddCommand<TWorld>> options)
        {
            var commands = new InputCommands<TWorld>();
            options(commands);
            return commands;
        }
    }
}