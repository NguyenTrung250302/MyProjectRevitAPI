using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace FirstProjectRevitAPIBasic.Utils
{
    public class Common
    {
        /// <summary>
        /// Lấy danh sach các BuiltInCategory cơ bản cho mô hình
        /// </summary>
        /// <returns></returns>
        public static List<BuiltInCategory> GetBasicModelCategories()
        {
            return new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Walls,          // Tường
                BuiltInCategory.OST_StructuralColumns, // Cột kết cấu
                BuiltInCategory.OST_Floors,         // Sàn
                BuiltInCategory.OST_Doors,          // Cửa đi
                BuiltInCategory.OST_Windows         // Cửa sổ
            };
        }

        /// <summary>
        /// Lấy tất cả các Level trong tài liệu Revit
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<Level> GetBasicModelLevels(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation) // Sắp xếp theo cao độ
                .ToList();
        }

        /// <summary>
        /// Lấy tất cả các phần tử theo Category và level cụ thể
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="category"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static List<Element> GetElementsByCategoryAndLevel(Document doc, BuiltInCategory category, Level level)
        {
            var collector = new FilteredElementCollector(doc)
                .OfCategory(category)
                .WhereElementIsNotElementType();

            if (level != null)
            {
                // Một số element có LevelId, một số có BuiltInParameter.LEVEL_PARAM, một số thì không
                var filtered = new List<Element>();

                foreach (var e in collector)
                {
                    // Ưu tiên dùng Element.LevelId nếu có
                    ElementId elementLevelId = ElementId.InvalidElementId;

                    try
                    {
                        elementLevelId = e.LevelId;
                    }
                    catch
                    {
                        // Nếu LevelId không có, thử lấy LEVEL_PARAM
                        Parameter levelParam = e.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                        if (levelParam != null && levelParam.StorageType == StorageType.ElementId)
                        {
                            elementLevelId = levelParam.AsElementId();
                        }
                    }

                    if (elementLevelId != ElementId.InvalidElementId && elementLevelId == level.Id)
                    {
                        filtered.Add(e);
                    }
                }

                return filtered;
            }

            // Nếu không truyền level thì trả toàn bộ theo category
            return collector.ToList();
        }

        /// <summary>
        /// Xóa các phần tử theo danh sách ElementId
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementIds"></param>
        public static void DeleteElements(Document doc, List<ElementId> elementIds)
        {
            if (elementIds == null || elementIds.Count == 0) return;

            using (Transaction trans = new Transaction(doc, "Delete Elements"))
            {
                trans.Start();
                doc.Delete(elementIds);
                trans.Commit();
            }
        }


        /// <summary>
        /// Sét giá trị Mark cho các phần tử theo danh sách ElementId
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementIds"></param>
        /// <param name="mark"></param>
        public static void SetMarkForElements(Document doc, List<ElementId> elementIds, string baseMark)
        {
            if (elementIds == null || elementIds.Count == 0 || string.IsNullOrEmpty(baseMark)) return;

            using (Transaction trans = new Transaction(doc, "Set Unique Mark and Color for Elements"))
            {
                trans.Start();

                int suffix = 1;
                HashSet<string> existingMarks = new HashSet<string>();

                // Lấy toàn bộ Mark hiện tại để tránh trùng
                var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                foreach (Element e in collector)
                {
                    Parameter markParam = e.LookupParameter("Mark");
                    if (markParam != null && markParam.StorageType == StorageType.String)
                    {
                        string val = markParam.AsString();
                        if (!string.IsNullOrEmpty(val))
                        {
                            existingMarks.Add(val);
                        }
                    }
                }

                foreach (ElementId id in elementIds)
                {
                    Element el = doc.GetElement(id);
                    if (el != null)
                    {
                        Parameter markParam = el.LookupParameter("Mark");
                        if (markParam != null && !markParam.IsReadOnly)
                        {
                            string uniqueMark;
                            do
                            {
                                uniqueMark = $"{baseMark}_{suffix++}";
                            }
                            while (existingMarks.Contains(uniqueMark));

                            markParam.Set(uniqueMark);
                            existingMarks.Add(uniqueMark);
                        }

                        // Highlight element
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        ogs.SetProjectionLineColor(new Color(255, 0, 0));
                        ogs.SetSurfaceBackgroundPatternColor(new Color(255, 200, 200));

                        if (!doc.ActiveView.IsTemplate)
                        {
                            doc.ActiveView.SetElementOverrides(id, ogs);
                        }
                    }
                }

                trans.Commit();
            }
        }



    }
}
