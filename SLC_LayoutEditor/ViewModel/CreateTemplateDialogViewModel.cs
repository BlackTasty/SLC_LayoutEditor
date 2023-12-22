namespace SLC_LayoutEditor.ViewModel
{
    internal class CreateTemplateDialogViewModel : AddEditDialogViewModel
    {
        public CreateTemplateDialogViewModel() :
            base("A template with this name exists already!")
        {

        }
    }
}