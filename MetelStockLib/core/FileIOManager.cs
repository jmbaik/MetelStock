using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MetelStockLib.core
{
    public class FileIOManager
    {
        public static string FILE_INTEREST_LIST = "interestlist.ser";

        //관심종목 리스트(그룹) 저장
        public static bool SaveInterestGroupList(List<Group> groupList)
        {
            try
            {
                using (FileStream fileStream = new FileStream(FILE_INTEREST_LIST, FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    bf.Serialize(fileStream, groupList);
                }

                return true;
            }
            catch (IOException ioException)
            {
                Console.WriteLine(ioException.Message);
                return false;
            }
        }

        public static List<Group> LoadInterestGroupList()
        {
            List<Group> groupList = null;
            try
            {
                using (FileStream fileStream = new FileStream(FILE_INTEREST_LIST, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    groupList = (List<Group>)bf.Deserialize(fileStream);
                }

                return groupList;
            }
            catch (IOException ioException)
            {
                Console.WriteLine(ioException.Message);
                return null;
            }
        }
    }
}
