using UnityEngine;

namespace TheProject
{
    // Gắn vào GO CHA (có collider). Tự động tìm tất cả Renderer
    // trong chính nó và các con → mỗi Renderer đều được bật/tắt outline.
    public class OutLineController : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterial;

        // Mỗi RendererEntry lưu Renderer + 2 bộ material (có/không outline)
        private struct RendererEntry
        {
            public Renderer renderer;
            public Material[] baseMats;
            public Material[] outlineMats;
        }

        private RendererEntry[] _entries;
        private bool _isOutlineEnabled = false;

        private void Awake()
        {
            // Tìm TẤT CẢ Renderer trong GO này và các con của nó
            Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

            _entries = new RendererEntry[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] baseMats = renderers[i].materials; // Instance copy, không phải shared reference

                // Tạo bộ material mới = base + outline material thêm vào cuối
                Material[] outlineMats = new Material[baseMats.Length + 1];
                baseMats.CopyTo(outlineMats, 0);
                outlineMats[outlineMats.Length - 1] = outlineMaterial;

                _entries[i] = new RendererEntry
                {
                    renderer = renderers[i],
                    baseMats = baseMats,
                    outlineMats = outlineMats
                };
            }
        }

        public void EnableOutline()
        {
            if (_isOutlineEnabled) return;
            foreach (var entry in _entries)
                entry.renderer.materials = entry.outlineMats;
            _isOutlineEnabled = true;
        }

        public void DisableOutline()
        {
            if (!_isOutlineEnabled) return;
            foreach (var entry in _entries)
                entry.renderer.materials = entry.baseMats;
            _isOutlineEnabled = false;
        }
    }
}
