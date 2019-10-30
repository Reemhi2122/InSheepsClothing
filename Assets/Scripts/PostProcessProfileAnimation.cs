using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.Scripts.PostProcessing
{
    public class PostProcessProfileAnimation: MonoBehaviour
    {
        private delegate void InterpolationMethod(ParameterOverride from, ParameterOverride to, float t); // copied from Unity source code

        public PostProcessVolume Target;

        [Range(0, 1)]
        public float Progress;

        public PostProcessProfile InitialState;
        public PostProcessProfile TargetState;

        private DateTime _started;

        private Dictionary<InterpolationMethod, (ParameterOverride, ParameterOverride)> Params = new Dictionary<InterpolationMethod, (ParameterOverride, ParameterOverride)>();

        void Start()
        {
            if (!InitialState.settings.Select(s => s.GetType())
                .SequenceEqual(TargetState.settings.Select(s => s.GetType())))
            {
                enabled = false;
                throw new Exception("Effects of starting and ending states dont match!");
            }

            Target.profile = ScriptableObject.CreateInstance<PostProcessProfile>();

            foreach (var type in InitialState.settings.Select(s => s.GetType()))
                Target.profile.AddSettings(type);

            for (var index = 0; index < InitialState.settings.Count; index++)
            {
                var initialSetting = InitialState.settings[index];
                var targetSettings = TargetState.settings[index];
                var realSettings = Target.profile.settings[index];
                
                var parameters = initialSetting.GetType().GetFields()
                    .Where(f => f.FieldType.IsSubclassOf(typeof(ParameterOverride))).ToArray();

                var interpolationMethod =
                    typeof(ParameterOverride).GetMethod("Interp", BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var fieldInfo in parameters)
                {
                    var iParamInstance = (ParameterOverride)fieldInfo.GetValue(initialSetting);
                    var tParamInstance = (ParameterOverride)fieldInfo.GetValue(targetSettings);
                    var rParamInstance = (ParameterOverride) fieldInfo.GetValue(realSettings);

                    rParamInstance.overrideState = iParamInstance.overrideState;

                    var interpMethodDelegate = (InterpolationMethod)Delegate.CreateDelegate(typeof(InterpolationMethod), rParamInstance, interpolationMethod, true);

                    Params.Add(interpMethodDelegate, (iParamInstance, tParamInstance));
                }
            }

            _started = DateTime.Now;
        }

        void Update()
        {
            foreach (var tuple in Params)
            {
                var interpMethodDelegate = tuple.Key;
                var iParam = tuple.Value.Item1;
                var tParam = tuple.Value.Item2;

                interpMethodDelegate(iParam, tParam, Progress);
            }
        }
    }
}