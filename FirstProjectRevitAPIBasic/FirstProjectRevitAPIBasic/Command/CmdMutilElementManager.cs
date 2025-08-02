using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using FirstProjectRevitAPIBasic.Utils;
using FirstProjectRevitAPIBasic.UI;
using Autodesk.Revit.Attributes;

namespace FirstProjectRevitAPIBasic.Command
{
    [Transaction(TransactionMode.Manual)]
    internal class CmdMutilElementManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Chưa có logic cụ thể, chỉ là một khung lệnh
            try
            {
                List<Level> levels = Common.GetBasicModelLevels(doc);
                if (levels.Count == 0)
                {
                    message = "Không tìm thấy bất kỳ Level nào được sử dụng trong tài liệu.";
                    return Result.Failed;
                }

                List<BuiltInCategory> categories = Common.GetBasicModelCategories();
                if (categories.Count == 0)
                {
                    message = "Không tìm thấy bất kỳ loại danh mục nào được sử dụng trong tài liệu.";
                    return Result.Failed;
                }
                FormMutil_ElementManager formMutilManager = new FormMutil_ElementManager(doc, categories, levels);
                formMutilManager.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}