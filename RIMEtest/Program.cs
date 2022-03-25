using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RIMEtest {
    class Program {
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern RimeApi rime_get_api();

        static void Main(string[] args) {
            Console.WriteLine("test,測試");
            var rime = rime_get_api();
            var traist = new RimeTraits();
            traist.app_name = "rime.testKK";
            rime.setup(ref traist);
            rime.set_notification_handler(on_message, System.IntPtr.Zero);

            var traist_null = new RimeTraits();
            rime.initialize(ref traist_null);

            bool full_check = true;
            if (rime.start_maintenance(full_check)) {
                rime.join_maintenance_thread();
            }
            Console.WriteLine("ready.");
            nuint session_id = rime.create_session();
            if (session_id == 0) {
                Console.WriteLine("Error creating rime session.");
                Console.ReadLine();
                return;
            }

            //const int kMaxLength = 99;
            //char line[kMaxLength + 1] = { 0 };
            string line;
            while (true) {
                line = Console.ReadLine();
                if (line.Equals("exit")) {
                    break;
                }
                if (execute_special_command(line, session_id)) {
                    continue;
                }
                if (rime.simulate_key_sequence(session_id, line)) {
                    print(session_id);
                } else {
                    Console.WriteLine("Error processing key sequence: %s", line);
                }

            }

            rime.destroy_session(session_id);
            rime.finalize();

            Console.WriteLine("Last line");
            Console.ReadLine();
        }
        private static void on_message(System.IntPtr context_object, nuint session_id, string message_type, string message_value) {
            Console.WriteLine(string.Format( "message: [{0}] [{1}] {2}",session_id, message_type, message_value));
        }
        private static void print(nuint session_id) {
            var rime = rime_get_api();
            var commit = new RimeCommit();
            var status = new RimeStatus();
            var context = new RimeContext();
            RIME_STRUCT(ref commit);
            RIME_STRUCT(ref status);
            RIME_STRUCT(ref context);

            if (rime.get_commit(session_id, ref commit)) {
                Console.WriteLine($"commit: {commit.text}");
                //rime.free_commit(ref commit);
            }

            if (rime.get_status(session_id, ref status)) {
                print_status(ref status);
                //rime.free_status(ref status);
            }

            if (rime.get_context(session_id, ref context)) {
                print_context(ref context);
                //rime.free_context(ref context);
            }

            return;
        }
        private static void print_status(ref RimeStatus status) {
            Console.WriteLine($"schema: {status.schema_id} / {status.schema_name}");
            Console.Write("status: ");
            if (status.is_disabled) Console.Write("disabled ");
            if (status.is_composing) Console.Write("composing ");
            if (status.is_ascii_mode) Console.Write("ascii ");
            if (status.is_full_shape) Console.Write("full_shape ");
            if (status.is_simplified) Console.Write("simplified ");
            Console.WriteLine();
            return;
        }
        private static void print_context(ref RimeContext context) {
            if (context.composition.length > 0) {
                print_composition(ref context.composition);
                print_menu(ref context.menu);
            } else {
                Console.WriteLine("(not composing)");
            }
        }
        private static void print_composition(ref RimeComposition composition) {

            string preedit = composition.preedit;
            if (string.IsNullOrEmpty(preedit)) return;
            int len = preedit.Length;
            int start = composition.sel_start;
            int end = composition.sel_end;
            int cursor = composition.cursor_pos;
            for (int i = 0; i <= len; ++i) {
                if (start < end) {
                    if (i == start) {
                        Console.Write('[');
                    } else if (i == end) {
                        Console.Write(']');
                    }
                }
                if (i == cursor) Console.Write('|');
                if (i < len)
                    Console.Write(preedit[i]);
            }
            Console.WriteLine();
        }
        private static void print_menu(ref RimeMenu menu) {
            if (menu.num_candidates == 0) return;
            Console.WriteLine(string.Format("page: {0}{1} (of size {2})",
                   menu.page_no + 1,
                   menu.is_last_page ? '$' : ' ',
                   menu.page_size));
            var itemSize = Marshal.SizeOf(typeof(RimeCandidate));
            var items = new RimeCandidate[menu.num_candidates];
            for (int i = 0; i < menu.num_candidates; ++i) {
                bool highlighted = i == menu.highlighted_candidate_index;
                IntPtr ins = new IntPtr(menu.candidates.ToInt64() + i * itemSize);
                items[i] = Marshal.PtrToStructure<RimeCandidate>(ins);
                Console.WriteLine(string.Format("{0}. {1}{2}{3}{4}",
                       i + 1,
                       highlighted ? '[' : ' ',
                       items[i].text,
                       highlighted ? ']' : ' ',
                       !string.IsNullOrEmpty(items[i].comment) ? items[i].comment : ""));
            }
        }
        private static bool execute_special_command(string line, nuint session_id) {
            var rime = rime_get_api();
            if (line.Equals("print schema list")) {
                var list = new RimeSchemaList();
                if (rime.get_schema_list(ref list)) {
                    Console.WriteLine("schema list:");
                    var itemSize = Marshal.SizeOf(typeof(RimeSchemaListItem));
                    var items = new RimeSchemaListItem[list.size];
                    for (int i = 0; i < (int)list.size; ++i) {
                        IntPtr ins = new IntPtr(list.list.ToInt64() + i * itemSize);
                        items[i] = Marshal.PtrToStructure<RimeSchemaListItem>(ins);
                        Console.WriteLine(string.Format("{0}. {1} [{2}]", i + 1, items[i].name, items[i].schema_id));
                    }
                    rime.free_schema_list(ref list);
                }
                char[] current = new char[100];
                if (rime.get_current_schema(session_id, ref current, current.Length)) {
                    Console.WriteLine(string.Format("current schema: {0}", new string(current)));
                }
                return true;
            }

            const string kSelectSchemaCommand = "select schema ";
            int command_length = kSelectSchemaCommand.Length;
            if (line.Contains(kSelectSchemaCommand) && line.Substring(0, command_length).Equals(kSelectSchemaCommand)) {
                string schema_id = line.Replace(kSelectSchemaCommand, "");
                if (rime.select_schema(session_id, schema_id)) {
                    Console.WriteLine(string.Format("selected schema: [{0}]", schema_id));
                }
                return true;
            }

            const string kSelectCandidateCommand = "select candidate ";
            command_length = kSelectCandidateCommand.Length;
            if (line.Contains(kSelectCandidateCommand) && line.Substring(0, command_length).Equals(kSelectCandidateCommand)) {
                line = line.Replace(kSelectCandidateCommand, "");
                int index = int.Parse(line) + command_length;
                if (index > 0 && rime.select_candidate_on_current_page(session_id, index -1)) {
                    Console.WriteLine(session_id);
                } else {
                    Console.WriteLine(string.Format("cannot select candidate at index {0}.", index));
                }
                return true;
            }
            if (line.Contains("print candidate list")) {
                var iterator = new RimeCandidateListIterator();
                if (rime.candidate_list_begin(session_id, ref iterator)) {
                    while (rime.candidate_list_next(ref iterator)) {
                        Console.Write(string.Format("{0}. {1}", iterator.index + 1, iterator.candidate.text));
                        if (!string.IsNullOrWhiteSpace(iterator.candidate.comment)) {
                            Console.Write($" ({iterator.candidate.comment})");
                        }
                        Console.WriteLine("");
                    }
                    rime.candidate_list_end(ref iterator);
                } else {
                    Console.WriteLine("no candidates.");
                }
                return true;
            }
            string kSetOptionCommand = "set option ";
            if (line.Contains(kSetOptionCommand)) {
                var is_on = true;
                var option = line.Replace(kSetOptionCommand, "");
                if (option.StartsWith("!")) {
                    is_on = false;
                    option = option.TrimStart('!');
                }
                rime.set_option(session_id, option, is_on);
                Console.Write($"{option} set ");
                if (is_on) {
                    Console.WriteLine("on.");
                } else {
                    Console.WriteLine("off.");
                }
                return true;
            }
            return false;
        }
        private static void RIME_STRUCT<T>(ref T value) {
            dynamic temp = value;
            temp.data_size = 0;
            value = temp;
            RIME_STRUCT_INIT(ref value);
        }
        private static void RIME_STRUCT_INIT<T>(ref T value) {
            dynamic temp = value;
            temp.data_size = Marshal.SizeOf(typeof(T));
            value = temp;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeTraits {
        public int data_size;
        [MarshalAs(UnmanagedType.LPStr)] public string shared_data_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string user_data_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string distribution_name;
        [MarshalAs(UnmanagedType.LPStr)] public string distribution_code_name;
        [MarshalAs(UnmanagedType.LPStr)] public string distribution_version;
        [MarshalAs(UnmanagedType.LPStr)] public string app_name;
        public IntPtr modules;
        public int min_log_level;
        [MarshalAs(UnmanagedType.LPStr)] public string log_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string prebuild_data_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string staging_dir;
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RimeNotificationHandler(System.IntPtr context_object, nuint session_id, string message_type, string message_value);
    //[StructLayout(LayoutKind.Sequential)]
    //public struct RimeNotificationHandler {
    //    [MarshalAs(UnmanagedType.LPStr)] public string context_object;
    //    public nuint session_id;
    //    [MarshalAs(UnmanagedType.LPStr)] public string message_type;
    //    [MarshalAs(UnmanagedType.LPStr)] public string message_value;
    //}
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeComposition {
        public int length;
        public int cursor_pos;
        public int sel_start;
        public int sel_end;
        [MarshalAs(UnmanagedType.LPStr)] public string preedit;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeCandidate {
        [MarshalAs(UnmanagedType.LPStr)] public string text;
        [MarshalAs(UnmanagedType.LPStr)] public string comment;
        public IntPtr reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeMenu {
        public int page_size;
        public int page_no;
        public bool is_last_page;
        public int highlighted_candidate_index;
        public int num_candidates;
        public IntPtr candidates;
        [MarshalAs(UnmanagedType.LPStr)] public string select_keys;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeCommit {
        public int data_size;
        [MarshalAs(UnmanagedType.LPStr)] public string text;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeContext {
        public int data_size;
        public RimeComposition composition;
        public RimeMenu menu;
        [MarshalAs(UnmanagedType.LPStr)] public string commit_text_preview;
        //char** select_labels;
        public IntPtr select_labels;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeStatus {
        public int data_size;
        [MarshalAs(UnmanagedType.LPStr)] public string schema_id;
        [MarshalAs(UnmanagedType.LPStr)] public string schema_name;
        public bool is_disabled;
        public bool is_composing;
        public bool is_ascii_mode;
        public bool is_full_shape;
        public bool is_simplified;
        public bool is_traditional;
        public bool is_ascii_punct;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeCandidateListIterator {
        public IntPtr ptr;
        public int index;
        public RimeCandidate candidate;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeConfig {
        public IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RimeConfigIterator {
        public IntPtr list;
        public IntPtr map;
        public int index;
        [MarshalAs(UnmanagedType.LPStr)] public string key;
        [MarshalAs(UnmanagedType.LPStr)] public string path;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RimeSchemaListItem {
        [MarshalAs(UnmanagedType.LPStr)] public string schema_id;
        [MarshalAs(UnmanagedType.LPStr)] public string name;
        public IntPtr reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeSchemaList {
        public nuint size;
        public IntPtr list;
    }
    // TODO line 191

}
