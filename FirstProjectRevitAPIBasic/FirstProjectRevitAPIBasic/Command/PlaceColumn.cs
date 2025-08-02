using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

namespace FirstProjectRevitAPIBasic.Command
{
    [Transaction(TransactionMode.Manual)]
    internal class PlaceColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Chọn điểm đặt cột
                XYZ point = uidoc.Selection.PickPoint("Chọn vị trí đặt cột");

                // 2. Lấy loại cột mặc định (FamilySymbol) - Structural Column
                FamilySymbol columnType = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .Cast<FamilySymbol>()
                    .FirstOrDefault();

                if (columnType == null)
                {
                    message = "Không tìm thấy loại cột.";
                    return Result.Failed;
                }

                // 3. Lấy Level mặc định
                Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault();

                if (level == null)
                {
                    message = "Không tìm thấy Level.";
                    return Result.Failed;
                }

                using (Transaction trans = new Transaction(doc, "Đặt cột"))
                {
                    trans.Start();

                    // 4. Đảm bảo FamilySymbol đã được kích hoạt
                    if (!columnType.IsActive)
                    {
                        columnType.Activate();
                        doc.Regenerate();
                    }

                    // 5. Đặt cột tại điểm đã chọn
                    FamilyInstance column = doc.Create.NewFamilyInstance(
                        point,
                        columnType,
                        level,
                        StructuralType.Column);

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
