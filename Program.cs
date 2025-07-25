// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace emby_crack
{
    public class Constants
    {
        public const string EMBY_VALIDATE_URL = "https://mb3admin.com";
    }

    public static class PatchUtils
    {
        /// <summary>
        /// 替换字节数组中 UTF-8 编码的字符串内容：将 "https://mb3admin.com" 替换为指定 embyCrackURL。
        /// </summary>
        /// <param name="inputBytes">原始字节数组（UTF-8 编码）</param>
        /// <param name="embyCrackURL">用于替换的 URL</param>
        /// <returns>修改后的字节数组</returns>
        public static byte[] PatchBytesReplaceMb3Admin(byte[] inputBytes, string embyCrackURL)
        {
            // 解码为字符串（假设 UTF-8 编码）
            string originalText = Encoding.UTF8.GetString(inputBytes);

            // 替换内容
            string modifiedText = originalText.Replace(Constants.EMBY_VALIDATE_URL, embyCrackURL);

            // 编码回字节数组
            return Encoding.UTF8.GetBytes(modifiedText);
        }
        /// <summary>
        /// 替换字符串中的内容
        /// </summary>
        /// <param name="original">原始字符串</param>
        /// <param name="embyCrackURL">用于替换的URL</param>
        /// <returns>修改后的字符串</returns>
        public static string PatchStringReplaceMb3Admin(string original, string embyCrackURL)
        {
            return original.Replace(Constants.EMBY_VALIDATE_URL, embyCrackURL);
        }
        /// <summary>
        /// 修改函数，使其直接return true;
        /// </summary>
        /// <param name="method">需要修改的函数</param>
        public static void PatchMethodReturnTrue(MethodDef method)
        {
            method.Body = new CilBody();
            var il = method.Body.Instructions;
            il.Add(OpCodes.Ldc_I4_1.ToInstruction()); // 推入布尔值 true
            il.Add(OpCodes.Ret.ToInstruction());      // 返回
        }
        public static bool PatchMethodLdstr(MethodDef method, string embyCrackURL)
        {
            int count = 0;
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldstr && instr.Operand is string s && s.Contains(Constants.EMBY_VALIDATE_URL))
                {
                    instr.Operand = PatchStringReplaceMb3Admin(s, embyCrackURL);
                    count++;
                }
            }
            if (count == 0)
            {
                return false;
            }
            return true;
        }
    }

    public class PathHelper
    {
        private readonly string embyBasePath;
        private readonly string outputPath;
        public PathHelper()
        {
            embyBasePath = Environment.GetEnvironmentVariable("EMBY_PATH") ?? "/app/emby/system";
            outputPath = Environment.GetEnvironmentVariable("DEBUG_OUTPUT_PATH") ?? embyBasePath;
        }

        public void DebugPrint()
        {
            Console.WriteLine($"环境变量 EMBY_PATH={embyBasePath}");
            Console.WriteLine($"环境变量 DEBUG_OUTPUT_PATH={outputPath}");
        }

        public string Path(string dllRelativePath)
        {
            return System.IO.Path.Combine(embyBasePath, dllRelativePath);
        }
        public string OutputPath(string dllRelativePath)
        {
            return System.IO.Path.Combine(outputPath, dllRelativePath);
        }
    }
    public class DNLibHelper
    {
        public static IEnumerable<TypeDef> GetAllNestedTypes(TypeDef type)
        {
            yield return type;
            foreach (var nested in type.NestedTypes)
            {
                foreach (var sub in GetAllNestedTypes(nested))
                    yield return sub;
            }
        }
    }

    public class TextFilePatcher
    {
        private string content;
        public TextFilePatcher(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("文件未找到", filePath);
            }

            content = File.ReadAllText(filePath);
            Console.WriteLine($"[加载] 文本文件: {filePath}");
        }

        public void ReplaceUrl(string embyCrackURL)
        {
            if (!content.Contains(Constants.EMBY_VALIDATE_URL))
            {
                throw new Exception($"文件不含 {Constants.EMBY_VALIDATE_URL}");
            }
            content = content.Replace(Constants.EMBY_VALIDATE_URL, embyCrackURL);
            Console.WriteLine($"[修改] 文本中的 {Constants.EMBY_VALIDATE_URL} 为 {embyCrackURL}");
        }

        public void Save(string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(outputPath, content);
            Console.WriteLine($"[保存] 修改后的文本到：{outputPath}");
        }

    }

    public class DLLPatcher
    {
        private readonly ModuleDefMD _module;

        public DLLPatcher(string dllPath)
        {
            _module = ModuleDefMD.Load(dllPath);
            Console.WriteLine($"[加载] dll文件：{dllPath}");
        }
        public void Save(string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _module.Write(outputPath);
            Console.WriteLine($"[保存] 修改后的 DLL 到：{outputPath}");
        }

        /// <summary>
        /// 替换指定嵌入资源中的 URL，目标为 "https://mb3admin.com"
        /// </summary>
        /// <param name="resourceName">资源完整名称</param>
        /// <param name="embyCrackURL">用于替换的新 URL</param>
        public void PatchResourceUrl(string resourceName, string embyCrackURL)
        {
            var resource = _module.Resources
                .OfType<EmbeddedResource>()
                .FirstOrDefault(r => string.Equals(r.Name, resourceName, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception($"资源 '{resourceName}' 未找到。");

            var reader = resource.CreateReader();
            var originalBytes = reader.ReadBytes((int)reader.Length);
            string originalText = Encoding.UTF8.GetString(originalBytes);

            if (!originalText.Contains(Constants.EMBY_VALIDATE_URL))
            {
                throw new Exception("资源中未找到需要替换的 URL");
            }

            // 替换
            string modifiedText = originalText.Replace(Constants.EMBY_VALIDATE_URL, embyCrackURL);
            byte[] modifiedBytes = Encoding.UTF8.GetBytes(modifiedText);

            // 替换资源
            _module.Resources.Remove(resource);
            var newRes = new EmbeddedResource(resourceName, modifiedBytes, ManifestResourceAttributes.Public);
            _module.Resources.Add(newRes);
            Console.WriteLine($"[修改] 替换{resourceName} 中的 {Constants.EMBY_VALIDATE_URL} 为 {embyCrackURL}");
        }

        public void PatchIsMBSupporterGetter()
        {
            // 找到类型
            var type = _module.Types
                .FirstOrDefault(t => t.FullName == "MediaBrowser.Model.Entities.PluginSecurityInfo");

            if (type == null)
            {
                throw new Exception("未找到类型 PluginSecurityInfo");
            }

            // 找到属性 getter
            var method = type.Methods.FirstOrDefault(m => m.Name == "get_IsMBSupporter");
            if (method == null)
            {
                throw new Exception("未找到方法 get_IsMBSupporter");
            }

            // 修改方法体：始终返回 true
            PatchUtils.PatchMethodReturnTrue(method);
            Console.WriteLine($"[修改] get_IsMBSupporter 始终返回true");
        }

        public void PatchConstString(string fieldName, string embyCrackURL)
        {
            bool modified = false;
            foreach (var type in _module.GetTypes())
            {
                foreach (var field in type.Fields)
                {
                    if (field.IsLiteral && field.HasConstant && field.Name == fieldName && field.Constant.Value is string original && original.Contains(Constants.EMBY_VALIDATE_URL))
                    {
                        field.Constant.Value = PatchUtils.PatchStringReplaceMb3Admin(original, embyCrackURL);
                        modified = true;
                        Console.WriteLine($"[修改] {field.DeclaringType.FullName}.{field.Name} 中的 {Constants.EMBY_VALIDATE_URL} 为 {embyCrackURL}");
                    }
                }
            }
            if (!modified)
            {
                throw new Exception($"未找到变量{fieldName}");
            }
        }
        public void PatchPluginSecurityManagerUpdateRegistrationStatus(string embyCrackURL)
        {
            var targetType = _module.Types
                .SelectMany(t => DNLibHelper.GetAllNestedTypes(t))
                .FirstOrDefault(t => t.FullName.Contains("PluginSecurityManager/<UpdateRegistrationStatus>d__26")) ?? throw new Exception("找不到PluginSecurityManager/<UpdateRegistrationStatus>d__26");

            var moveNextMethod = targetType.Methods.FirstOrDefault(m => m.Name == "MoveNext") ?? throw new Exception("找不到 MoveNext 方法");
            if (!PatchUtils.PatchMethodLdstr(moveNextMethod, embyCrackURL))
            {
                throw new Exception("找不到ldstr");
            }
            Console.WriteLine($"[修改] PluginSecurityManager/<PluginSecurityManager>d_26 中的ldstr 中的 {Constants.EMBY_VALIDATE_URL} 为 {embyCrackURL}");
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            string embyCrackURL = Environment.GetEnvironmentVariable("EMBY_CRACK_URL") ?? throw new Exception("环境变量 EMBY_CRACK_URL 未设置");
            Console.WriteLine($"环境变量 EMBY_CRACK_URL={embyCrackURL}");

            var pathHelper = new PathHelper();
            pathHelper.DebugPrint();

            string jsRelativePath;
            // 处理embypremiere.js
            jsRelativePath = "dashboard-ui/embypremiere/embypremiere.js";
            var jsPatcher = new TextFilePatcher(pathHelper.Path(jsRelativePath));
            jsPatcher.ReplaceUrl(embyCrackURL);
            jsPatcher.Save(pathHelper.OutputPath(jsRelativePath));
            // 处理connectionmanager.js
            jsRelativePath = "dashboard-ui/modules/emby-apiclient/connectionmanager.js";
            jsPatcher = new TextFilePatcher(pathHelper.Path(jsRelativePath));
            jsPatcher.ReplaceUrl(embyCrackURL);
            jsPatcher.Save(pathHelper.OutputPath(jsRelativePath));

            string dllRelativePath;
            // 处理 Emby.Web.dll
            dllRelativePath = "Emby.Web.dll";
            string resourceName = "Emby.Web.dashboard_ui.modules.emby_apiclient.connectionmanager.js";
            var dllPatcher = new DLLPatcher(pathHelper.Path(dllRelativePath));
            dllPatcher.PatchResourceUrl(resourceName, embyCrackURL);
            dllPatcher.Save(pathHelper.OutputPath(dllRelativePath));

            // 处理 MediaBrowser.Model.dll
            dllRelativePath = "MediaBrowser.Model.dll";
            dllPatcher = new DLLPatcher(pathHelper.Path(dllRelativePath));
            dllPatcher.PatchIsMBSupporterGetter();
            dllPatcher.Save(pathHelper.OutputPath(dllRelativePath));

            // 处理 Emby.Server.Implementations.dll
            dllRelativePath = "Emby.Server.Implementations.dll";
            dllPatcher = new DLLPatcher(pathHelper.Path(dllRelativePath));
            dllPatcher.PatchConstString("MBValidateUrl", embyCrackURL);
            dllPatcher.PatchPluginSecurityManagerUpdateRegistrationStatus(embyCrackURL);
            dllPatcher.Save(pathHelper.OutputPath(dllRelativePath));
        }
    }
}