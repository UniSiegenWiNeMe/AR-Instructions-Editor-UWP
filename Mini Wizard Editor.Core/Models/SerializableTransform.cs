namespace ARInstructionsEditor.Core.Models
{
    public class SerializableTransform
    {
        public float[] Position = new float[3];
        public float[] Rotation = new float[4];
        public float[] Scale = new float[3];
        public bool WorldSpace = false;
    }
}