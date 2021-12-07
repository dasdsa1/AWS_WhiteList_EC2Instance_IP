using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda1
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(object input, ILambdaContext context)
        {
            context.Logger.Log("initializing \n");
            

            
            
            var security = new SecurityGroupManager("AWS_SECRET", "AWS_KEY", RegionEndpoint.EUCentral1);

            context.Logger.LogLine("Created security group.");

            var mySGs = security.EnumerateSecurityGroups(security._ec2Client);

            List<IpRange> ranges = new List<IpRange>();
            
            //Get the IP address from EC instance with ID "i-0bb4c9dbec3772fbe"
            var addressesResponse = security._ec2Client.DescribeAddressesAsync();


            foreach (var address in addressesResponse.Result.Addresses)
            {


                if (address.InstanceId == "InstanceId1")
                {
                    
                    ranges.Add(new IpRange { CidrIp = address.PrivateIpAddress + "/32", Description = "" });
                }
                if (address.InstanceId == "InstanceId2")
                {   
                   
                    ranges.Add(new IpRange { CidrIp = address.PrivateIpAddress + "/32", Description = "" });
                }
                
            }

            var securityGroup = security.CreateEc2SecurityGroup(security._ec2Client, "TestSecurityGroup");

            context.Logger.LogLine("Deleting");
            security.DeleteSecurityGroupIngress(securityGroup.Result);
            context.Logger.LogLine("Deleted");

            List<int> PortsList = new List<int>()
            {
                1870
            };

            foreach (var port in PortsList)
            {
                context.Logger.LogLine("Creating");
                security.CreateSecurityGroupIngress(port, ranges, securityGroup.Result);
                context.Logger.LogLine("Created");
            }


        }
    }
}
