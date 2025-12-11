using Course31_trpo.Sources.LoadModules;
using Course31_trpo.Sources.Structures;
using ObservableCollections;
using R3;

namespace Course31_trpo.VM
{
    public class LoadedItemsVM
    {
        public ObservableList<GroupCollection<string, Report>> ModuleList { get; }

        public LoadedItemsVM()
        {
            ModuleList = [];
            foreach (IImportModule module in MauiProgram.ImportModules) { module.LoadedItems.ObserveChanged().Subscribe(_ => Update()); }
            Update();
        }

        private void Update()
        {
            ModuleList.Clear();
            foreach (IImportModule module in MauiProgram.ImportModules) { ModuleList.Add(new(module.Name, module.LoadedItems)); }
        }
    }
}
