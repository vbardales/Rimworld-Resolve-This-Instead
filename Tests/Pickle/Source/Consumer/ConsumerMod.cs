using System.Linq;
using Verse;

namespace ResolveThisInstead.TestConsumer
{
    /// <summary>
    /// Plays the part of mod "C" of a chain C asks for A, A is replaced by B: its Mod constructor, like many real ones, asks whether A is loaded
    /// and looks for A's Mod handle (the source of the "Mod options" list). A is absent, B is loaded. The constructor must see A as absent:
    /// aliases answer only from LoadModXML on, never while Mod constructors run, or C would go on to use A's types and A's settings.
    /// </summary>
    public class ConsumerMod : Mod
    {
        public const string OldPackageId = "adaptive.storage.neolithic";

        public static bool? IsActiveOldInConstructor;
        public static bool? OldModHandleInConstructor;
        public static bool? GetActiveOldInConstructor;

        public ConsumerMod(ModContentPack content) : base(content)
        {
            IsActiveOldInConstructor = ModsConfig.IsActive(OldPackageId);
            GetActiveOldInConstructor = ModLister.GetActiveModWithIdentifier(OldPackageId, ignorePostfix: true) != null;
            OldModHandleInConstructor = LoadedModManager.ModHandles.Any(h => h.Content != null && string.Equals(h.Content.PackageId, OldPackageId, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Plays C's own runtime code (a settings window, a gameplay check), called well after loading, from a Pickle step: this method's
        /// BODY runs inside ResolveThisInstead.TestConsumer.dll, so the library's caller-assembly detection sees a mod, not the engine, and
        /// (per Runtime.MayAnswer's third window) should answer as if A were loaded. Recorded rather than asserted here: the step that calls
        /// this is not itself C's code, only this method is, and the point is to test exactly that boundary.
        /// </summary>
        public static bool? IsActiveOldAfterLoad;

        public static void CheckAfterLoad()
        {
            IsActiveOldAfterLoad = ModsConfig.IsActive(OldPackageId);
        }
    }
}
