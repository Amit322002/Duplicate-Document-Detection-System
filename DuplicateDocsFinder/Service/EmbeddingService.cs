using OpenCvSharp;

namespace DuplicateDocsFinder.Service
{
    public class EmbeddingService
    {
        public float[] GenerateEmbedding(byte[] imageBytes)
        {
            using var img = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);

            if (img.Empty())
                return null;

            Cv2.Resize(img, img, new Size(32, 32));

            var vector = new float[img.Rows * img.Cols];

            int index = 0;

            for (int i = 0; i < img.Rows; i++)
            {
                for (int j = 0; j < img.Cols; j++)
                {
                    vector[index++] = img.At<byte>(i, j) / 255f;
                }
            }

            return vector;
        }
    }
}