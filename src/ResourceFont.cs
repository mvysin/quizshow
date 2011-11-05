using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;

namespace Vysin.QuizShow
{
    class ResourceFontCollection : IDisposable
    {
        private PrivateFontCollection fonts;
        private List<IntPtr> fontPtrs; 

        public ResourceFontCollection()
        {
            fonts = new PrivateFontCollection();
            fontPtrs = new List<IntPtr>();
        }

        ~ResourceFontCollection()
        {
            Dispose(false);
        }

        public void AddResource(byte[] font)
        {
            IntPtr bfr = Marshal.AllocHGlobal(font.Length);
            Marshal.Copy(font, 0, bfr, font.Length);
            fonts.AddMemoryFont(bfr, font.Length);
            fontPtrs.Add(bfr);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            fonts.Dispose();
            foreach (IntPtr i in fontPtrs)
                Marshal.FreeHGlobal(i);
            fontPtrs.Clear();
        }

        public FontFamily GetPrivateFont(string familyName)
        {
            foreach (FontFamily f in fonts.Families)
            {
                if (f.Name == familyName)
                    return f;
            }
            return null;
        }
    }
}
