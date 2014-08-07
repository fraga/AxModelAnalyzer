﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel.Data.ModelDependency
{
    public class ModelDependency
    {
        public int ModelId { get; private set; }
        public int LayerId { get; private set; }
        public string ModelName { get; private set; }
        public string LayerName { get; private set; }

        public int BaseModelId { get; private set; }
        public int BaseLayerId { get; private set; }
        public string BaseModelName { get; private set; }
        public string BaseLayerName { get; private set; }

        private Data.AX_2012_R2_modelEntities db;
        public ModelDependency(ref Data.AX_2012_R2_modelEntities db, int modelId, int baseModelId)
        {
            this.db = db;

            this.ModelId = modelId;
            this.BaseModelId = baseModelId;

            this.populateLayersAndNames();
        }

        public string ToCSVString()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", this.ModelId, this.ModelName, this.LayerId, this.LayerName, this.BaseModelId, this.BaseModelName, this.BaseLayerId, this.BaseLayerName);
        }
        private static string CSVHeader()
        {
            return "Model Id, Model name, Layer Id, Layer name, Base model id, Base model name, Base layerId, Base layer name";
        }
        private void populateLayersAndNames()
        {
            //using (var db = new AX_2012_R2_modelEntities())
            {
                //get the layer ids for the models
                var model = this.db.Models.First(m => m.Id == this.ModelId);
                this.LayerId = model.LayerId;
                this.ModelName = this.db.ModelManifests.First(m => m.ModelId == this.ModelId).Name;
                this.LayerName = model.Layer.Name;

                //get the layer / names for the base model
                if (this.BaseLayerId > 0)
                {
                    var baseModel = this.db.Models.First(m => m.Id == this.BaseModelId);
                    this.BaseLayerId = baseModel.LayerId;
                    this.BaseModelName = this.db.ModelManifests.First(m => m.ModelId == this.BaseModelId).Name;
                    this.BaseLayerName = baseModel.Layer.Name;
                }
                else
                {
                    this.BaseLayerId = 0;
                    this.BaseModelName = string.Empty;
                    this.BaseLayerName = string.Empty;
                }
            }
        }

        /// <summary>
        /// Returns if a model dependency exists
        /// </summary>
        /// <param name="modelsDep"></param>
        /// <returns></returns>
        public static bool Exists(List<ModelDependency> modelsDep, int modelId, int baseModelid)
        {
            bool ret = false;

            if (modelsDep != null && modelsDep.Count > 0)
            {
                var count = modelsDep.Count(m => m.ModelId == modelId && m.BaseModelId == baseModelid);
                if (count > 0)
                    ret = true;
            }

            return ret;
        }

        public static List<string> GetDependenciesCSV(List<ModelDependency> dependencies)
        {
            List<string> lines = new List<string>();

            if (dependencies != null && dependencies.Count > 0)
            {
                lines.Add(ModelDependency.CSVHeader());
                var deps = dependencies.OrderBy(d => d.LayerId).ThenBy(d => d.BaseLayerId).ToList();

                deps.ForEach(d => lines.Add(d.ToCSVString()));
            }
            return lines;
        }
    }
}
