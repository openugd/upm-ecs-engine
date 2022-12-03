﻿using System;
using System.Collections.Generic;
using OpenUGD.ECS.Engine.Inputs;
using OpenUGD.ECS.Engine.Outputs;

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

    public abstract class Engine<TWorld> : ECSEngine where TWorld : OpenUGD.ECS.World
    {
        public abstract EngineOutputs EngineOutputs { get; }
        public abstract TWorld? World { get; }
    }

    public class Engine<TWorld, TConfiguration> : Engine<TWorld>, IECSEngine, IEngineState
        where TWorld : OpenUGD.ECS.World
        where TConfiguration : EngineConfiguration
    {
        private readonly Queue<Input> _playedInputs;
        private readonly EngineInputs<TWorld> _inputs;
        private readonly EngineOutputs _outputs;
        private readonly EngineSystems<TWorld> _systems;
        private readonly IExternalProvider<TWorld, TConfiguration> _externalProvider;
        private readonly Context _context;

        private int _seed;
        private int _tick;
        private TWorld? _world;

        class Context : IEngineContext
        {
            public int Tick;
            public int Seed { get; set; }
            public EngineOutputs Outputs;
            public T Enqueue<T>() where T : Output, new()
            {
                return Outputs.Enqueue<T>(Tick);
            }
        }

        public Engine(TConfiguration configuration, IExternalProvider<TWorld, TConfiguration> externalProvider)
        {
            Configuration = configuration;

            ExternalResolver = _externalProvider = externalProvider;
            _seed = configuration.RandomSeed;
            _playedInputs = new Queue<Input>();
            _outputs = new EngineOutputs();
            _inputs = new EngineInputs<TWorld>(this, externalProvider.GetCommandsProvider());
            _systems = new EngineSystems<TWorld>(externalProvider.GetEngineSystemsProvider());
            _context = new Context
            {
                Outputs = _outputs,
            };

            InitializeState(_seed);

            if (configuration.Tick < 0)
            {
                throw new ArgumentException("tick must be >= 0");
            }

            if (configuration.Inputs != null)
            {
                foreach (var action in configuration.Inputs)
                {
                    Inputs.AddInput(action);
                }
            }
            
            var nextTick = Math.Max(configuration.Tick, 0);
            
            _context.Tick = nextTick;

            _systems.Initialize(_world!, _context);

            _externalProvider.Start(this, _world!);

            FastForward(nextTick);
        }

        public IExternalResolver ExternalResolver { get; }
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
        public sealed override EngineOutputs EngineOutputs => _outputs;

        public override EngineSnapshot Snapshot()
        {
            var snapshot = new EngineSnapshot(Configuration, _playedInputs.ToArray(), _seed);
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

                    _externalProvider.Start(this, _world!);
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

            _externalProvider.Start(this, _world!);

            var nextTick = Math.Max(Configuration.Tick, 0);
            FastForward(nextTick);
        }

        public void Continue() => ExitState = ExitReason.None;

        public void Exit(ExitReason reason) => ExitState = reason;

        public void SetTick(int currentTick) => _tick = currentTick;

        public override void Dispose()
        {
            if (World != null)
            {
                for (var i = 0; i < World.SubWorldCount; i++)
                {
                    var entities = World.Pool.PopEntityIds(World.SubWorldUnsafe[i]);
                    foreach (var entity in entities)
                    {
                        World.DeleteEntity(entity);
                    }

                    World.Pool.Return(entities);
                }
            }
        }

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
            _world = _externalProvider.CreateWorld(this, seed);
        }
    }
}