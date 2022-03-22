using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RIMEtest {
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeApi {
        public int data_size;
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeSetup(ref RimeTraits traits);
        public void setup(ref RimeTraits traits) {
            RimeSetup(ref traits);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        //public static extern void RimeSetNotificationHandler(Action<System.IntPtr, nuint, string, string> handler, System.IntPtr context_object);
        public static extern void RimeSetNotificationHandler(RimeNotificationHandler handler, System.IntPtr context_object);
        public void set_notification_handler(RimeNotificationHandler handler, System.IntPtr context_object) {
            RimeSetNotificationHandler(handler, context_object);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeInitialize(ref RimeTraits traits);
        public void initialize(ref RimeTraits traits) {
            RimeInitialize(ref traits);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeStartMaintenance(bool full_check);
        public bool start_maintenance(bool full_check) {
            return RimeStartMaintenance(full_check);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeJoinMaintenanceThread();
        public void join_maintenance_thread() {
            RimeJoinMaintenanceThread();
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern nuint RimeCreateSession();
        public nuint create_session() {
            return RimeCreateSession();
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeSimulateKeySequence(nuint session_id, string key_sequence);
        public bool simulate_key_sequence(nuint session_id, string key_sequence) {
            return RimeSimulateKeySequence(session_id, key_sequence);
        }
        [DllImport("rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetSchemaList(ref RimeSchemaList schemaList);
        public bool get_schema_list(ref RimeSchemaList schemaList) {
            return RimeGetSchemaList(ref schemaList);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeFreeSchemaList(ref RimeSchemaList schemaList);
        public void free_schema_list(ref RimeSchemaList schema) {
            RimeFreeSchemaList(ref schema);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetCommit(nuint session_id, ref RimeCommit commit);
        public bool get_commit(nuint session_id, ref RimeCommit commit) {
            return RimeGetCommit(session_id, ref commit);
        }
        [DllImport("rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetCurrentSchema(nuint session_id, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 100)] char[] schema_id, int buffer_size);
        public bool get_current_schema(nuint session_id, ref char[] schema_id, int buffer_size) {
            return RimeGetCurrentSchema(session_id, schema_id, buffer_size);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeFreeCommit(ref RimeCommit commit);
        public bool free_commit(ref RimeCommit commit) {
            return RimeFreeCommit(ref commit);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeSelectSchema(nuint session_id, string schema_id);
        public bool select_schema(nuint session_id, string schema_id) {
            return RimeSelectSchema(session_id, schema_id);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetStatus(nuint session_id, ref RimeStatus status);
        public bool get_status(nuint session_id, ref RimeStatus status) {
            return RimeGetStatus(session_id, ref status);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeFreeStatus(ref RimeStatus status);
        public bool free_status(ref RimeStatus status) {
            return RimeFreeStatus(ref status);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetContext(nuint session_id, ref RimeContext context);
        public bool get_context(nuint session_id, ref RimeContext context) {
            return RimeGetContext(session_id, ref context);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeFreeContext(ref RimeContext context);
        public bool free_context(ref RimeContext context) {
            return RimeFreeContext(ref context);
        }

        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeSetupLogging(ref string app_name);
        public void setup_logging(ref string app_name) {
            RimeSetupLogging(ref app_name);
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeFinalize();
        public void finalize() {
            RimeFinalize();
        }
        [DllImport("rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeDestroySession(nuint session_id);
        public bool destroy_session(nuint session_id) {
            return RimeDestroySession(session_id);
        }
    }
}
