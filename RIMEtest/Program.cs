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
            //rime.set_notification_handler(on_message, System.IntPtr.Zero);

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
            Console.WriteLine("KLog: print");
            var rime = rime_get_api();
            var commit = new RimeCommit();
            var status = new RimeStatus();
            var context = new RimeContext();

            if (rime.get_commit(session_id, ref commit)) {
                Console.WriteLine("commit: %s", commit.text);
                rime.free_commit(ref commit);
            } else { Console.WriteLine("KLog: fail get_commit"); }

            if (rime.get_status(session_id, ref status)) {
                print_status(ref status);
                rime.free_status(ref status);
            } else { Console.WriteLine("KLog: fail get_status"); }

            if (rime.get_context(session_id, ref context)) {
                print_context(ref context);
                rime.free_context(ref context);
            } else { Console.WriteLine("KLog: fail get_context"); }

            return;
        }
        private static void print_status(ref RimeStatus status) {
            Console.WriteLine("schema: %s / %s",
                   status.schema_id, status.schema_name);
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
            Console.WriteLine("page: %d%c (of size %d)",
                   menu.page_no + 1,
                   menu.is_last_page ? '$' : ' ',
                   menu.page_size);
            for (int i = 0; i < menu.num_candidates; ++i) {
                bool highlighted = i == menu.highlighted_candidate_index;
                Console.WriteLine("%d. %c%s%c%s",
                       i + 1,
                       highlighted ? '[' : ' ',
                       menu.candidates[i].text,
                       highlighted ? ']' : ' ',
                       !string.IsNullOrEmpty(menu.candidates[i].comment) ? menu.candidates[i].comment : "");
            }
        }
        private static bool execute_special_command(string line, nuint session_id) {
            var rime = rime_get_api();
            if (line.Equals("print schema list")) {
                var list = new RimeSchemaList();
                    if (rime.get_schema_list(ref list)) {
                    Console.WriteLine("schema list:");
                    for (int i = 0; i < (int)list.size; ++i) {
                        //printf("%lu. %s [%s]\n", (i + 1),
                        //       list.list[i].name, list.list[i].schema_id);
                        Console.Write(string.Format("{0}. {1} [{2}]", i + 1, list.list[i].name, list.list[i].schema_id));
                    }
                    rime.free_schema_list(ref list);
                }
                //    char current[100] = { 0 };
                //    if (rime->get_current_schema(session_id, current, sizeof(current))) {
                //        printf("current schema: [%s]\n", current);
                //    }
                return true;
            }

            //  const char* kSelectSchemaCommand = "select schema ";
            //size_t command_length = strlen(kSelectSchemaCommand);
            //if (!strncmp(line, kSelectSchemaCommand, command_length)) {
            //    const char* schema_id = line + command_length;
            //    if (rime->select_schema(session_id, schema_id)) {
            //        printf("selected schema: [%s]\n", schema_id);
            //    }
            //    return true;
            //}

            //const char* kSelectCandidateCommand = "select candidate ";
            //command_length = strlen(kSelectCandidateCommand);
            //if (!strncmp(line, kSelectCandidateCommand, command_length)) {
            //    int index = atoi(line + command_length);
            //    if (index > 0 &&
            //        rime->select_candidate_on_current_page(session_id, index - 1)) {
            //        print(session_id);
            //    } else {
            //        fprintf(stderr, "cannot select candidate at index %d.\n", index);
            //    }
            //    return true;
            //}

            //if (!strcmp(line, "print candidate list")) {
            //    RimeCandidateListIterator iterator = { 0 };
            //    if (rime->candidate_list_begin(session_id, &iterator)) {
            //        while (rime->candidate_list_next(&iterator)) {
            //            printf("%d. %s", iterator.index + 1, iterator.candidate.text);
            //            if (iterator.candidate.comment)
            //                printf(" (%s)", iterator.candidate.comment);
            //            putchar('\n');
            //        }
            //        rime->candidate_list_end(&iterator);
            //    } else {
            //        printf("no candidates.\n");
            //    }
            //    return true;
            //}
            //const char* kSetOptionCommand = "set option ";
            //command_length = strlen(kSetOptionCommand);
            //if (!strncmp(line, kSetOptionCommand, command_length)) {
            //    Bool is_on = True;
            //    const char* option = line + command_length;
            //    if (*option == '!') {
            //        is_on = False;
            //        ++option;
            //    }
            //    rime->set_option(session_id, option, is_on);
            //    printf("%s set %s.\n", option, is_on ? "on" : "off");
            //    return true;
            //}
            return false;
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
        //void* reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeMenu {
        public int page_size;
        public int page_no;
        public bool is_last_page;
        public int highlighted_candidate_index;
        public int num_candidates;
        public RimeCandidate[] candidates;
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
        //void* ptr;
        public int index;
        public RimeCandidate candidate;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeConfig {
        //void* ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RimeConfigIterator {
        //void* list;
        //void* map;
        public int index;
        [MarshalAs(UnmanagedType.LPStr)] public string key;
        [MarshalAs(UnmanagedType.LPStr)] public string path;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RimeSchemaListItem {
        [MarshalAs(UnmanagedType.LPStr)] public string schema_id;
        [MarshalAs(UnmanagedType.LPStr)] public string name;
        //void* reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RimeSchemaList {
        public nuint size;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1)]
        public RimeSchemaListItem[] list;
    }
    // TODO line 191

}
