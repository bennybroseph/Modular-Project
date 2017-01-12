using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Library;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
public class ProportionRenderer : ExecuteInEditor
{
    private delegate float GetFloat();

    public enum ValueAttribute
    {
        Current,
        Delayed,
        Additive,
        Subtractive
    }
    [Serializable]
    private class ValueContainer
    {
        public Image parentImage = null;
        public bool useParentValues;

        [Space]
        public string valueName;
        public string minValue;
        public string maxValue;

        [Space]
        public AnimationCurve animationCurve;

        [Space]
        public ValueAttribute attribute;    // Cannot be the parent and also be additive

        [HideInNormalInspector]
        public Image image;

        // 'internal' prevents variables from being show in the inspector unless Debug mode is enabled
        // In C# internal allows only the current assembly access to these variables
        // In other words, the 'Editor' project isn't allowed access to it to serialize it
        internal ValueContainer parentContainer;

        internal PropertyInfo valueProperty;
        internal GetFloat valueDelegate;
        internal bool hasProperty;

        internal PropertyInfo minValueProperty;
        internal bool hasMinProperty;
        internal float minValueParse;
        internal PropertyInfo maxValueProperty;
        internal bool hasMaxProperty;
        internal float maxValueParse;

        internal float previousFill;
        internal float currentFill;
        internal bool coroutineIsRunning;
        internal float timeSiceLastChange;
    }
    [SerializeField, Range(0, 100)]
    private float m_TestingValue;
    public float testingValue
    {
        get { return m_TestingValue; }
        set { m_TestingValue = value; }
    }

    [SerializeField]
    private Component m_Parent;
    [SerializeField]
    private bool m_AutoUpdate = false;

    [Space]
    [SerializeField]
    private List<ValueContainer> m_Values = new List<ValueContainer>();

    public Component parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    private void OnValidate()
    {
        foreach (var value in m_Values)
            ValidateValueProperty(value);
    }
    private void ValidateValueProperty(ValueContainer a_Value)
    {
        if (a_Value.parentImage != null)
            foreach (var valueContainer in m_Values)
                if (valueContainer.image == a_Value.parentImage)
                {
                    a_Value.parentContainer = valueContainer;

                    if (!a_Value.useParentValues)
                        continue;

                    a_Value.valueName = valueContainer.valueName;
                    a_Value.minValue = valueContainer.minValue;
                    a_Value.maxValue = valueContainer.maxValue;
                }

        float result;

        if (float.TryParse(a_Value.valueName, out result) == false && a_Value.valueName != null)
            a_Value.valueProperty = m_Parent.GetType().GetProperty(a_Value.valueName);
        else
            a_Value.valueProperty = null;

        if (float.TryParse(a_Value.minValue, out result) == false && a_Value.minValue != null)
            a_Value.minValueProperty = m_Parent.GetType().GetProperty(a_Value.minValue);
        else
        {
            a_Value.minValueParse = result;
            a_Value.minValueProperty = null;
        }

        if (float.TryParse(a_Value.maxValue, out result) == false && a_Value.maxValue != null)
            a_Value.maxValueProperty = m_Parent.GetType().GetProperty(a_Value.maxValue);
        else
        {
            a_Value.maxValueParse = result;
            a_Value.maxValueProperty = null;
        }

        a_Value.hasProperty = a_Value.valueProperty != null;
        a_Value.hasMinProperty = a_Value.minValueProperty != null;
        a_Value.hasMaxProperty = a_Value.maxValueProperty != null;

        if (a_Value.hasProperty)
            a_Value.valueDelegate =
                Delegate.CreateDelegate(
                    typeof(GetFloat), m_Parent,
                    a_Value.valueProperty.GetGetMethod()) as GetFloat;
    }

    // Use this for initialization
    private void Awake()
    {
        //if (m_Values == null)
        //    m_Values = new List<ValueContainer>();

        if (m_AutoUpdate && m_Values.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(UpdateValues());
        }
    }

    protected override void OnEditorStart()
    {
        OnValidate();
        Awake();
    }
    protected override void OnEditorUpdateSelected()
    {
        OnEditorUpdate();
    }
    protected override void OnEditorUpdate()
    {
        GetImages();
        OnValidate();
    }

    protected override void OnGameStart() { }
    protected override void OnGameUpdate() { }

    private void GetImages()
    {
        // Remove deleted image references
        var tempList = m_Values.ToList();
        foreach (var value in tempList)
            if (value.image == null)
                m_Values.Remove(value);

        tempList = m_Values.ToList();
        foreach (var childImage in GetComponentsInChildren<Image>())
        {
            bool foundImage = false;
            foreach (var value in tempList)
            {
                if (value.image == childImage)
                {
                    foundImage = true;
                    break;
                }
            }
            if (!foundImage)
                m_Values.Add(new ValueContainer { image = childImage });
        }
    }

    [ContextMenu("List Properties")]
    private void ListProperties()
    {
        if (m_Parent == null)
        {
            Debug.Log("Parent is currently set to null");
            return;
        }

        foreach (var info in m_Parent.GetType().GetProperties())
        {
            Debug.Log(info);
        }
    }

    private IEnumerator UpdateValues()
    {
        while (true)
        {
            foreach (var value in m_Values.Where(x => x.hasProperty && x.image != null))
            {
                var parsedCurrentValue = value.valueDelegate();

                var parsedMinValue =
                                value.hasMinProperty
                                    ? (float)value.minValueProperty.GetValue(m_Parent, null)
                                    : value.minValueParse;
                var parsedMaxValue =
                    value.hasMaxProperty
                        ? (float)value.maxValueProperty.GetValue(m_Parent, null)
                        : value.maxValueParse;

                switch (value.attribute)
                {
                    case ValueAttribute.Current:
                        {
                            value.image.fillAmount =
                                (parsedCurrentValue - parsedMinValue) / (parsedMaxValue - parsedMinValue);
                        }
                        break;
                    case ValueAttribute.Delayed:
                        {
                            if (value.currentFill < value.parentImage.fillAmount)
                                value.currentFill = value.parentImage.fillAmount;

                            if (Math.Abs(value.previousFill - parsedCurrentValue) > float.Epsilon)
                            {
                                SetAdjustedFill(value, value.currentFill);
                                value.image.transform.position =
                                    new Vector3(
                                        value.parentImage.transform.position.x
                                        + value.parentImage.rectTransform.sizeDelta.x * value.parentImage.fillAmount,
                                        value.parentImage.transform.position.y,
                                        value.parentImage.transform.position.z);

                                value.timeSiceLastChange = 0f;
                                if (!value.coroutineIsRunning)
                                    StartCoroutine(ReduceFillAmount(value));
                            }

                            value.previousFill = parsedCurrentValue;
                        }
                        break;
                    case ValueAttribute.Additive:
                        {


                            value.image.fillAmount =
                                (parsedCurrentValue - parsedMinValue) / (parsedMaxValue - parsedMinValue);
                            break;
                        }
                    case ValueAttribute.Subtractive:
                        break;
                }
            }
            yield return null;
        }
    }
    private IEnumerator ReduceFillAmount(ValueContainer a_Value)
    {
        a_Value.coroutineIsRunning = true;

        var amountToReduce = a_Value.currentFill - a_Value.parentContainer.image.fillAmount;
        var originalFill = a_Value.currentFill;

        var deltaTime = 0f;
        while (deltaTime < a_Value.animationCurve[a_Value.animationCurve.length - 1].time)
        {
            while (a_Value.timeSiceLastChange < float.Epsilon)
            {
                a_Value.timeSiceLastChange += Time.deltaTime;

                deltaTime = 0;

                amountToReduce = a_Value.currentFill - a_Value.parentContainer.image.fillAmount;
                originalFill = a_Value.currentFill;

                yield return false;
            }

            deltaTime += Time.deltaTime;

            a_Value.currentFill =
                originalFill
                - a_Value.animationCurve.Evaluate(deltaTime)
                * amountToReduce;

            SetAdjustedFill(a_Value, a_Value.currentFill);
            yield return false;
        }
        a_Value.coroutineIsRunning = false;
    }

    private void SetAdjustedFill(ValueContainer a_Value, float a_FillAmount)
    {
        a_Value.image.fillAmount = a_FillAmount
            - a_Value.parentImage.rectTransform.sizeDelta.x * a_Value.parentImage.fillAmount
            / a_Value.parentImage.rectTransform.sizeDelta.x;
    }
}
