using Cerpent.Core.Contract;

namespace Cerpent.Core.StereotypeRecognizer;

public class StereotypeRecognizer
{
    private IStereotypeDefinitionSource StereotypeDefinitionSource { get; }
    public StereotypeRecognizer(IStereotypeDefinitionSource stereotypeDefinitionSource)
    {
        StereotypeDefinitionSource = stereotypeDefinitionSource;
    }

    public StereotypeConfirmedResult[] FuzzySearch(Event @event)
    {
        var descriptions = StereotypeDefinitionSource.Get(@event.Name);
        var result = descriptions.Where(d =>
        {
            if (d.Accuracy == 0 || d.LowerBounds == null && d.UpperBounds == null)
                return true;

            return false;
        });
        
        return new StereotypeConfirmedResult[] { };
    }
}