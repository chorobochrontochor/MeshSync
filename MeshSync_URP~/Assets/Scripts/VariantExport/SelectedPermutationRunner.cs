﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.MeshSync.VariantExport
{
    internal class SelectedPermutationRunner : PermutationRunnerBase
    {
        public override long VariantCount
        {
            get
            {
                return regenerator.Whitelist.Count;
            }
        }

        public SelectedPermutationRunner(Regenerator regenerator) : base(regenerator)
        {
        }

        protected override IEnumerator ExecutePermutations(int propIdx)
        {
            foreach (var setting in regenerator.Whitelist)
            {
                if (!regenerator.IsBaking)
                {
                    yield break;
                }

                regenerator.ApplySerializedProperties(setting);

                yield return Save();
            }

            yield break;
        }
    }
}
