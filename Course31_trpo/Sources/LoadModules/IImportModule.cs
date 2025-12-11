using BloodShadow.Core.Operations;
using Course31_trpo.Sources.Structures;
using ObservableCollections;

namespace Course31_trpo.Sources.LoadModules
{
    public interface IImportModule
    {
        public string Name { get; }
        public IReadOnlyObservableList<Report> LoadedItems { get; }
        public ActionOperation Load(bool useDefaultDir = false);
    }
}
