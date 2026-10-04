using System;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data
{
    [Serializable]
    public class GraphParameter
    {
        [SerializeField]
        private string name = "NewParameter";
        [SerializeField]
        private ParameterId id;
        [SerializeField]
        private ParameterValueType valueType = ParameterValueType.Float;
        [SerializeField]
        private ParameterMode mode = ParameterMode.Constant;
        [SerializeField]
        private ParameterValue constantOrDefault;
        [SerializeField]
        private ParameterValue rangeMin;
        [SerializeField]
        private ParameterValue rangeMax;

        public string Name => name;
        public ParameterId Id => id;
        public ParameterValueType ValueType => valueType;
        public ParameterMode Mode => mode;
        public ParameterValue ConstantOrDefault => constantOrDefault;
        public ParameterValue RangeMin => rangeMin;
        public ParameterValue RangeMax => rangeMax;

        public GraphParameter()
        {
            constantOrDefault = ParameterValue.FromFloat(1f);
            rangeMin = ParameterValue.FromFloat(0.5f);
            rangeMax = ParameterValue.FromFloat(2f);
            id = ParameterId.FromString(name);
        }

        public GraphParameter(string paramName, ParameterValueType type, ParameterValue defaultValue)
        {
            name = paramName;
            valueType = type;
            constantOrDefault = defaultValue;
            rangeMin = defaultValue;
            rangeMax = defaultValue;
            id = ParameterId.FromString(name);
        }

        public void ValidateId()
        {
            id = ParameterId.FromString(name);
        }

        public void SetName(string paramName)
        {
            name = string.IsNullOrWhiteSpace(paramName) ? "NewParameter" : paramName.Trim();
            ValidateId();
        }

        public void SetMode(ParameterMode newMode) => mode = newMode;

        public void SetValueType(ParameterValueType newType)
        {
            if (valueType == newType)
                return;

            valueType = newType;
            ParameterValue defaults = ParameterValue.DefaultFor(newType);
            constantOrDefault = defaults;
            rangeMin = defaults;
            rangeMax = defaults;
        }

        public void SetConstant(ParameterValue value)
        {
            constantOrDefault = EnsureType(value);
        }

        public void SetRange(ParameterValue min, ParameterValue max)
        {
            rangeMin = EnsureType(min);
            rangeMax = EnsureType(max);
        }

        public ParameterValue Evaluate(ShapeContext context)
        {
            switch (mode)
            {
                case ParameterMode.Constant:
                case ParameterMode.Exposed:
                    return EnsureType(constantOrDefault);

                case ParameterMode.RandomRange:
                    return EvaluateRandom(context);

                default:
                    return EnsureType(constantOrDefault);
            }
        }

        private ParameterValue EvaluateRandom(ShapeContext context)
        {
            switch (valueType)
            {
                case ParameterValueType.Float:
                {
                    float min = rangeMin.Type == ParameterValueType.Float ? rangeMin.AsFloat() : 0f;
                    float max = rangeMax.Type == ParameterValueType.Float ? rangeMax.AsFloat() : 1f;
                    return ParameterValue.FromFloat(context.NextRange(id, min, max));
                }
                case ParameterValueType.Int:
                {
                    int min = rangeMin.Type == ParameterValueType.Int ? rangeMin.AsInt() : 0;
                    int max = rangeMax.Type == ParameterValueType.Int ? rangeMax.AsInt() : 1;
                    return ParameterValue.FromInt(context.NextIntInclusive(id, min, max));
                }
                case ParameterValueType.Bool:
                    return ParameterValue.FromBool(context.Next01(id) >= 0.5f);

                case ParameterValueType.Vector2:
                {
                    Vector2 min = rangeMin.Type == ParameterValueType.Vector2 ? rangeMin.AsVector2() : Vector2.zero;
                    Vector2 max = rangeMax.Type == ParameterValueType.Vector2 ? rangeMax.AsVector2() : Vector2.one;
                    float tx = DeterministicRng.Hash01(context.GlobalSeed, id.Hash, 1);
                    float ty = DeterministicRng.Hash01(context.GlobalSeed, id.Hash, 2);
                    return ParameterValue.FromVector2(new Vector2(
                        Mathf.Lerp(min.x, max.x, tx),
                        Mathf.Lerp(min.y, max.y, ty)));
                }
                case ParameterValueType.Color:
                {
                    Color min = rangeMin.Type == ParameterValueType.Color ? rangeMin.AsColor() : Color.black;
                    Color max = rangeMax.Type == ParameterValueType.Color ? rangeMax.AsColor() : Color.white;
                    float tr = DeterministicRng.Hash01(context.GlobalSeed, id.Hash, 1);
                    float tg = DeterministicRng.Hash01(context.GlobalSeed, id.Hash, 2);
                    float tb = DeterministicRng.Hash01(context.GlobalSeed, id.Hash, 3);
                    float ta = DeterministicRng.Hash01(context.GlobalSeed, id.Hash, 4);
                    return ParameterValue.FromColor(new Color(
                        Mathf.Lerp(min.r, max.r, tr),
                        Mathf.Lerp(min.g, max.g, tg),
                        Mathf.Lerp(min.b, max.b, tb),
                        Mathf.Lerp(min.a, max.a, ta)));
                }
                default:
                    return ParameterValue.DefaultFor(valueType);
            }
        }

        private ParameterValue EnsureType(ParameterValue value) =>
            value.Type == valueType ? value : ParameterValue.DefaultFor(valueType);
    }
}
