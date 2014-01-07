using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions;

namespace GAPPSF.UIControls.FormulaSolver
{
    public class FunctionDescriptor
    {
        public enum FunctionGroup
        {
            NumberGroup,
            TextGroup,
            CoordinateGroup,
            ContextGroup,
            TrigonometricGroup,
            NoGroup
        };

        public static string GroupName(FunctionGroup group) {
            switch (group)
            {
                case FunctionGroup.NumberGroup:
                    return StrRes.GetString(StrRes.STR_NUMBER_GROUP);
                case FunctionGroup.CoordinateGroup:
                    return StrRes.GetString(StrRes.STR_COORDINATE_GROUP);
                case FunctionGroup.TextGroup:
                    return StrRes.GetString(StrRes.STR_TEXT_GROUP);
                case FunctionGroup.ContextGroup:
                    return StrRes.GetString(StrRes.STR_CONTEXT_GROUP);
                case FunctionGroup.TrigonometricGroup:
                    return StrRes.GetString(StrRes.STR_TRIGONOMETRIC_GROUP);
                default:
                    return null;
            }
        }

        public string Name { get; private set; }
        public FunctionGroup Group { get; private set; }
        public string[] Alternates { get; private set; }
        public Functor Functor { get; private set; }
        public string Description { get; private set; }

        public FunctionDescriptor(string theName, FunctionGroup theGroup, string[] theAlternates, Functor theFunctor, string description)
        {
            Name = theName;
            Group = theGroup;
            Alternates = theAlternates;
            Functor = theFunctor;
            Description = description;
        }
    }
}
