using UnityEngine;

namespace GameSupportEditor
{
    public interface IMeshSaver
    {
        void Save(Mesh m, Material mat, string path , string fileName);
    }
}