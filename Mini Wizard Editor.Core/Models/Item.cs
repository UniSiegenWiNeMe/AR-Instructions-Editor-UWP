namespace ARInstructionsEditor.Core.Models
{
    public enum ItemType
    {
        Arrow = 0,
        Box,
        CircleArrow,
        ArrowWithToolTip,
        Unknown
    }


    public class Item
    {
        public ItemType Type { get; set; }
        public SerializableTransform Transform { get; set; }
        public float[] Color { get; set; }
        public bool IsActive { get; set; }
        public bool HasText{ get; set; }
        public string Text { get; set; }
        public float[] TextPosition { get; set; }
    }
}