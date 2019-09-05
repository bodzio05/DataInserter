using Confidence.API.ServiceModel.Services;
using Confidence.Platform.ServiceModel.Data;
using Confidence.Platform.Workstation.Applications.ServiceClient;
using Confident_ConfidenceTools.ConfidenceWrappers.Interfaces.Materials;
using Confident_ConfidenceTools.ConfidenceWrappers.Materials;
using Confident_DatabaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace Tools
{
    public static class ConfidenceTools
    {
        private static IProductDataServiceHost ProductClient => new LocalProductDataServiceClient().GetInterface();

        private static IProjectDataServiceHost ProjectClient => new LocalProjectDataServiceClient().GetInterface();

        public static IEnumerable<Material> SelectSingleMaterial(string name)
        {
            IMaterial material = null;
            try
            {
                material = MaterialAdapter.CreateMaterial(ProductClient.GetMaterialByName(name));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                yield break;
            }

            yield return material.Unwrap();
        }

        public static IEnumerable<IMaterial> SelectSingleProjectSpecific(string name, bool includeStandardMaterials)
        {
            yield return MaterialAdapter.CreateMaterial(ProjectClient.GetMaterialForCurrentProjectByName(name, includeStandardMaterials));
        }

        public static IEnumerable<IMaterial> SelectRandomMaterials(int amount) => SelectRandom(SelectAllMaterials(), amount);

        public static IEnumerable<IMaterial> SelectRandom(int amount) => SelectRandom(SelectAll(), amount);

        public static IEnumerable<IMaterial> SelectRandomProjectSpecifics(int amount) => SelectRandom(SelectAllProjectSpecifics(), amount);

        public static IEnumerable<IMaterial> SelectAll()
        {
            var materialList = MaterialAdapter.CreateMaterials(ProductClient.GetMaterials());
            var projectSpecificList = MaterialAdapter.CreateMaterials(ProjectClient.GetMaterials(GetDefaultProjectId()));
            return materialList.Concat(projectSpecificList);
        }

        public static IEnumerable<MaterialStandard> SelectAllStandards() => ProductClient.GetMaterialStandards();

        public static IEnumerable<IMaterial> SelectAllMaterials() => MaterialAdapter.CreateMaterials(ProductClient.GetMaterials());

        public static IEnumerable<IMaterial> SelectAllProjectSpecifics() => MaterialAdapter.CreateMaterials(ProjectClient.GetMaterials(GetDefaultProjectId()));

        public static bool CheckConnection()
        {
            if (!DatabaseData.Connection)
                return false;
            return true;
        }

        public static string GetCurrentEnvironment()
        {
            try
            {
                Confident_ConfidenceTools.ServiceInfo.SetDefaultProject();
                return Confident_ConfidenceTools.ServiceInfo.GetEnvironmentType();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static int GetDefaultProjectId()
        {
            try
            {
                Confident_ConfidenceTools.ServiceInfo.SetDefaultProject();
                return Confident_ConfidenceTools.ServiceInfo.ProjectId;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string GetDefaultProjectName()
        {
            try
            {
                Confident_ConfidenceTools.ServiceInfo.SetDefaultProject();
                return Confident_ConfidenceTools.ServiceInfo.ProjectName;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetCompanyName()
        {
            try
            {
                Confident_ConfidenceTools.ServiceInfo.SetDefaultProject();
                return Confident_ConfidenceTools.ServiceInfo.GetCompanyCode();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static IEnumerable<IMaterial> SelectRandom(IEnumerable<IMaterial> materials, int amount)
        {
            var random = new Random();
            int index;
            var list = new List<IMaterial>();
            for (int i = 0; i < amount; i++)
            {
                do index = random.Next(materials.Count());
                while (MaterialHasEmptyStandard(materials.ToList()[index]));

                list.Add(materials.ToList()[index]);
            }
            return list;
        }

        public static bool MaterialHasEmptyStandard(IMaterial material) => material.Primitives == null || material.Primitives.Count() == 0;

    }
}
