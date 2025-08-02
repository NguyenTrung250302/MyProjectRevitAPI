using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

namespace FirstProjectRevitAPIBasic.Command
{
    [Transaction(TransactionMode.Manual)]
    internal class PlaceWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Chọn 2 điểm từ người dùng
                XYZ point1 = uidoc.Selection.PickPoint("Chọn điểm đầu tường");
                XYZ point2 = uidoc.Selection.PickPoint("Chọn điểm cuối tường");

                // 2. Lấy loại tường mặc định (loại đầu tiên tìm thấy)
                WallType wallType = new FilteredElementCollector(doc)
                    .OfClass(typeof(WallType))
                    .Cast<WallType>()
                    .FirstOrDefault(wt => wt.Kind == WallKind.Basic);

                if (wallType == null)
                {
                    message = "Không tìm thấy loại tường.";
                    return Result.Failed;
                }

                // 3. Lấy level mặc định (level đầu tiên)
                Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault();

                if (level == null)
                {
                    message = "Không tìm thấy level.";
                    return Result.Failed;
                }

                // 4. Tạo đường Line giữa 2 điểm
                Line wallLine = Line.CreateBound(point1, point2);

                // 5. Bắt đầu transaction để tạo tường
                using (Transaction trans = new Transaction(doc, "Tạo tường từ 2 điểm"))
                {
                    trans.Start();

                    Wall wall = Wall.Create(doc, wallLine, wallType.Id, level.Id, 10.0, 0.0, false, false);
                    // 10.0 là chiều cao, 0.0 là offset, false: không flip, không structural

                    trans.Commit();
                }

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
