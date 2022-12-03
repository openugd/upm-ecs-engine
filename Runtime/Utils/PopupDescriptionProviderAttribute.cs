using System;

namespace OpenUGD.ECS.Engine.Utils
{
    public class PopupDescriptionProviderAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        /// <param name="pathToDescriptionList">path to array in descriptions, examples Bullets</param>
        /// <param name="namesProvider">names to show in popup, example: Id,Descriptions</param>
        /// <param name="valueField">value provider to set, example: Id</param>
        public PopupDescriptionProviderAttribute(string pathToDescriptionList, string namesProvider, string valueField)
        {
            PathToDescriptionList = pathToDescriptionList;
            NamesProvider = namesProvider;
            ValueField = valueField;
        }

        public string PathToDescriptionList { get; }
        public string NamesProvider { get; }
        public string ValueField { get; }
    }
}