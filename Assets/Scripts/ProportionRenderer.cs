using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Library;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProportionRenderer : ExecuteInEditor
{
    [Serializable]
    private class ValueContainer
    {
        public string valueName;
        public Image valueImage;
        public string minValue;
        public string maxValue;

        // 'internal' prevents variables from being show in the inspector unless Debug mode is enabled
        internal PropertyInfo valueProperty;
        internal bool hasProperty;

        internal PropertyInfo minValueProperty;
        internal bool hasMinProperty;
        internal int minValueParse;
        internal PropertyInfo maxValueProperty;
        internal bool hasMaxProperty;
        internal int maxValueParse;

    }

    [SerializeField]
    private Component m_Parent;
    [SerializeField]
    private bool m_AutoUpdate = false;

    [Space]
    [SerializeField]
    private List<ValueContainer> m_Values;

    public Component parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    private void OnValidate()
    {
        foreach (var value in m_Values)
        {
            int result;

            if (int.TryParse(value.valueName, out result) == false && value.valueName != null)
                value.valueProperty = m_Parent.GetType().GetProperty(value.valueName);
            else
                value.valueProperty = null;

            if (int.TryParse(value.minValue, out result) == false && value.minValue != null)
                value.minValueProperty = m_Parent.GetType().GetProperty(value.minValue);
            else
            {
                value.minValueParse = result;
                value.minValueProperty = null;
            }

            if (int.TryParse(value.maxValue, out result) == false && value.maxValue != null)
                value.maxValueProperty = m_Parent.GetType().GetProperty(value.maxValue);
            else
            {
                value.maxValueParse = result;
                value.maxValueProperty = null;
            }

            value.hasProperty = value.valueProperty != null;
            value.hasMinProperty = value.minValueProperty != null;
            value.hasMaxProperty = value.maxValueProperty != null;
        }
    }

    // Use this for initialization
    private void Awake()
    {
        if (m_Values == null)
            m_Values = new List<ValueContainer>();

        if (m_AutoUpdate && m_Values.Count > 0)
            StartCoroutine(UpdateValues());
    }

    protected override void OnEditorStart()
    {
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
        var images = 
            GetComponentsInChildren<Image>().
                Where(x => x.transform.parent == transform && x.type == Image.Type.Filled).ToArray();

        foreach (var image in images)
        {
            var foundImage = false;
            foreach (var valueContainer in m_Values)
            {
                if (valueContainer.valueImage != image)
                    continue;

                foundImage = true;
                break;
            }
            if (foundImage)
                continue;

            var newValue = new ValueContainer { valueImage = image };
            m_Values.Add(newValue);
        }

        ValueContainer[] tempValueContainers = new ValueContainer[m_Values.Count];
        m_Values.CopyTo(tempValueContainers);
        foreach (var valueContainer in tempValueContainers)
        {
            var foundImage = false;
            foreach (var image in images)
            {
                if (valueContainer.valueImage != image)
                    continue;

                foundImage = true;
                break;
            }
            if (!foundImage)
                m_Values.Remove(valueContainer);
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
            foreach (var value in m_Values.Where(x => x.hasProperty && x.valueImage != null))
            {
                var parsedCurrentValue = (float)value.valueProperty.GetValue(m_Parent, null);

                var parsedMinValue =
                    value.hasMinProperty
                        ? (float)value.minValueProperty.GetValue(m_Parent, null)
                        : value.minValueParse;
                var parsedMaxValue =
                    value.hasMaxProperty
                        ? (float)value.maxValueProperty.GetValue(m_Parent, null)
                        : value.maxValueParse;

                value.valueImage.fillAmount =
                    (parsedCurrentValue - parsedMinValue) / (parsedMaxValue - parsedMinValue);
            }
            yield return null;
        }
    }
}
