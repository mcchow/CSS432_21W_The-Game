using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameProtocol
{
    public class CategoryChoice : MessageType
    {
        public override string MessageID()
        {
            return "CategoryChoice";
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

    public class CategoryChoiceReader : MessageReader
    {
        public override MessageType ParseMessage(out int readSize, byte[] buffer, int offset, int bufferSize)
        {
            readSize = 0;
            return new CategoryChoice();
        }
    }


    public class ChooseCategory : MessageType
    {
        public string Category;

        public override string MessageID()
        {
            return "ChoseCategory";
        }

        public override byte[] ToBytes()
        {
            byte[] bytes = new Byte[Category.Length+1];
            bytes[Category.Length] = (byte)'\0';
            Encoding.UTF8.GetBytes(Category, 0, Category.Length, bytes, 4);
            return bytes;
        }
    }

    public class ChooseCategoryReader : MessageReader
    {
        private string Category = "";
        public override MessageType ParseMessage(out int readSize, byte[] buffer, int offset, int bufferSize)
        {
            bool foundNul = false;
            readSize = bufferSize;
            for (int i = offset; i < offset + bufferSize; ++i)
            {
                if (buffer[i] == (byte)'\0')
                {
                    readSize = i - offset;
                    break;
                }
            }
            string recieved = Encoding.UTF8.GetString(buffer, offset, readSize);
            if (foundNul)
            {
                ChooseCategory choice = new ChooseCategory();
                choice.Category = Category;
            }
            return null;
        }
    }
}
