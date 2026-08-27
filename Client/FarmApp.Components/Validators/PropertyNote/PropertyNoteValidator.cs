using FarmApp.Components.Components.PropertyNoteCreate;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components.Forms;

namespace FarmApp.Components.Validators.PropertyNote
{
    public static class PropertyNoteValidator
    {
        public static bool Validate(
            EditContext editContext,
            ValidationMessageStore messageStore,
            PropertyNotePreviewModel model,
            PropertyNoteCreateModal page,
            string propertyId)
        {
            messageStore.Clear();
            var isValid = editContext.Validate();

            if (string.IsNullOrWhiteSpace(propertyId))
            {
                messageStore.Add(
                    new FieldIdentifier(page, nameof(propertyId)),
                    "Please, choose the field");
                isValid = false;
            }

            if (model.StartTime.Date < DateTime.Today)
            {
                messageStore.Add(
                    new FieldIdentifier(model, nameof(model.StartTime)),
                    "Selected date must not be in the past");
                isValid = false;
            }

            if (model.EndTime < model.StartTime)
            {
                messageStore.Add(
                    new FieldIdentifier(model, nameof(model.EndTime)),
                    "Start time must be earlier than end time");
                isValid = false;
            }

            editContext.NotifyValidationStateChanged();
            return isValid;
        }
    }
}
