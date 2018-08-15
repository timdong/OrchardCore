using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class EnumerationFieldDisplayDriver : ContentFieldDisplayDriver<EnumerationField>
    {
        public EnumerationFieldDisplayDriver(IStringLocalizer<EnumerationFieldDisplayDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(EnumerationField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayEnumerationFieldViewModel>("EnumerationField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(EnumerationField field, BuildFieldEditorContext context)
        {
            return Initialize<EditEnumerationFieldViewModel>("EnumerationField_Edit", model =>
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<EnumerationFieldSettings>();
                var options = (!String.IsNullOrWhiteSpace(settings.Options)) ? settings.Options.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None) : new string[] { T["Select an option"].Value };

                var optionsSelected = new List<OptionViewModel>();
                foreach (var option in options)
                {
                    bool selected = field.SelectedValues.Contains(option);
                    optionsSelected.Add(new OptionViewModel { Value = option, Selected = selected });
                }

                model.Value = field.Value;
                model.Values = field.SelectedValues;
                model.Options = optionsSelected;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(EnumerationField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditEnumerationFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, f => f.Value, f => f.Values, f => f.Options))
            {
                if (model.Value != null)
                {
                    field.Value = model.Value;
                }

                if (model.Options != null)
                {
                    field.SelectedValues = model.Options.Where(x => x.Selected == true)?.Select(s => s.Value).ToArray();
                }

                if (model.Values != null)
                {
                    field.SelectedValues = model.Values;
                }
            }

            return Edit(field, context);
        }
    }
}