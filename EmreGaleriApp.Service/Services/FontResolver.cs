using PdfSharp.Fonts;
using System.IO;

namespace EmreGaleriApp.Service.Services
{
    public class FontResolver : IFontResolver
    {
        private readonly string _fontPath;

        public FontResolver(string fontPath)
        {
            _fontPath = fontPath;
        }

        public byte[] GetFont(string faceName)
        {
            return File.ReadAllBytes(_fontPath);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Arial", System.StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("Arial#");

            return new FontResolverInfo("Arial#");
        }
    }
}
