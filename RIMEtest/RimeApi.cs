using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RIMEtest {
    [StructLayout(LayoutKind.Sequential)]
    public class RimeApix {
        public int data_size;
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern void RimeSetup(RimeTraits traits);
        public void setup(RimeTraits traits) {
            RimeSetup(traits);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern char RimeGetVersion();
        [DllImport("NativeLibrary.dll")]
        private static extern IntPtr CreateWishList(string name);

        [DllImport("NativeLibrary.dll")]
        private static extern void DeleteWishList(IntPtr wishListPointer);

        [DllImport("NativeLibrary.dll")]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string GetWishListName(IntPtr wishListPointer);

        [DllImport("NativeLibrary.dll")]
        private static extern void SetWishListName(IntPtr wishListPointer, string name);

        [DllImport("NativeLibrary.dll")]
        private static extern void AddWishListItem(IntPtr wishListPointer, string name);

        [DllImport("NativeLibrary.dll")]
        private static extern void RemoveWishListItem(IntPtr wishListPointer, string name);

        [DllImport("NativeLibrary.dll")]
        private static extern int CountWishListItems(IntPtr wishListPointer);

        [DllImport("NativeLibrary.dll")]
        private static extern void PrintWishList(IntPtr wishListPointer);

        private readonly IntPtr _wishListPointer;

        public string Name {
            get {
                return GetWishListName(_wishListPointer);
            }
            set {
                SetWishListName(_wishListPointer, value);
            }
        }


    }
}
