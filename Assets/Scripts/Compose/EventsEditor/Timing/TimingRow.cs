using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class TimingRow : Row<TimingEvent>
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private TMP_InputField bpmField;
        [SerializeField] private TMP_InputField divisorField;

        public override bool Highlighted
        {
            get => highlight.activeSelf;
            set => highlight.SetActive(value);
        }

        public override void RemoveReference()
        {
            Reference = null;
            timingField.SetTextWithoutNotify(string.Empty);
            bpmField.SetTextWithoutNotify(string.Empty);
            divisorField.SetTextWithoutNotify(string.Empty);
            highlight.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            timingField.interactable = interactable && (Reference?.Timing != 0);
            bpmField.interactable = interactable;
            divisorField.interactable = interactable;

            if (!interactable)
            {
                highlight.SetActive(false);
            }
        }

        public override void SetReference(TimingEvent datum)
        {
            Reference = datum;
            timingField.SetTextWithoutNotify(datum.Timing.ToString());
            bpmField.SetTextWithoutNotify(datum.Bpm.ToString());
            divisorField.SetTextWithoutNotify(datum.Divisor.ToString());
            timingField.interactable = datum.Timing != 0;
        }

        private void Awake()
        {
            timingField.onEndEdit.AddListener(OnField);
            bpmField.onEndEdit.AddListener(OnField);
            divisorField.onEndEdit.AddListener(OnField);

            timingField.onSelect.AddListener(OnFieldSelect);
            bpmField.onSelect.AddListener(OnFieldSelect);
            divisorField.onSelect.AddListener(OnFieldSelect);
        }

        private void OnDestroy()
        {
            timingField.onEndEdit.RemoveListener(OnField);
            bpmField.onEndEdit.RemoveListener(OnField);
            divisorField.onEndEdit.RemoveListener(OnField);

            timingField.onSelect.RemoveListener(OnFieldSelect);
            bpmField.onSelect.RemoveListener(OnFieldSelect);
            divisorField.onSelect.RemoveListener(OnFieldSelect);
        }

        private void OnFieldSelect(string arg)
        {
            Table.Selected = Reference;
        }

        private void OnField(string arg)
        {
            if (Evaluator.TryInt(timingField.text, out int timing)
             && Evaluator.TryFloat(bpmField.text, out float bpm)
             && Evaluator.TryFloat(divisorField.text, out float divisor))
            {
                if (timing != Reference.Timing || bpm != Reference.Bpm || divisor != Reference.Divisor)
                {
                    TimingEvent newValue = new TimingEvent()
                    {
                        Timing = Reference.Timing == 0 ? 0 : (timing == 0 ? 1 : timing),
                        Bpm = bpm,
                        Divisor = divisor,
                        TimingGroup = Reference.TimingGroup,
                    };

                    Services.History.AddCommand(new EventCommand(
                        name: I18n.S("Compose.Notify.History.EditTiming"),
                        update: new List<(ArcEvent instance, ArcEvent newValue)> { (Reference, newValue) }));

                    ((TimingTable)Table).Rebuild();
                }
            }

            timingField.SetTextWithoutNotify(Reference.Timing.ToString());
            bpmField.SetTextWithoutNotify(Reference.Bpm.ToString());
            divisorField.SetTextWithoutNotify(Reference.Divisor.ToString());
            ((TimingTable)Table).UpdateMarker();
        }
    }
}