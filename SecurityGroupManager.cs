using System;
using System.Collections.Generic;
using System.Text;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Lambda.Core;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon;
using Amazon.ElasticFileSystem.Model;
using CreateTagsRequest = Amazon.EC2.Model.CreateTagsRequest;

namespace AWSLambda1
{
    
    public class SecurityGroupManager
    {
        public readonly AmazonEC2Client _ec2Client;
        private readonly BasicAWSCredentials _awsCredentials;
        protected RegionEndpoint AwsRegion { get; set; }
        
        public SecurityGroupManager(string awsKey, string awsSecret, RegionEndpoint awsRegion)
        {
            AwsRegion = awsRegion;
            _awsCredentials = new BasicAWSCredentials(awsKey, awsSecret);
            _ec2Client = new AmazonEC2Client(_awsCredentials, awsRegion);
        }

        public async Task<List<SecurityGroup>> EnumerateSecurityGroups(AmazonEC2Client ec2Client)
        {
            var request = new DescribeSecurityGroupsRequest();
            var response = await ec2Client.DescribeSecurityGroupsAsync(request);
            List<SecurityGroup> mySGs = response.SecurityGroups;

            return mySGs;
        }

        public async Task<SecurityGroup> CreateEc2SecurityGroup(AmazonEC2Client ec2Client, string secGroupName)
        {
            // See if a security group with the specified name already exists
            Filter nameFilter = new Filter();
            nameFilter.Name = "group-name";
            nameFilter.Values = new List<string>() { secGroupName };

            var describeRequest = new DescribeSecurityGroupsRequest();
            describeRequest.Filters.Add(nameFilter);
            var describeResponse = await ec2Client.DescribeSecurityGroupsAsync(describeRequest);

            
            // If a match was found, return the SecurityGroup object for the security group
            if (describeResponse.SecurityGroups.Count > 0)
            {
                return describeResponse.SecurityGroups[0];
            }

            // Create the security group
            var createRequest = new CreateSecurityGroupRequest();
            createRequest.GroupName = secGroupName;
            createRequest.Description = "My sample security group for EC2-Classic";

            var createResponse = ec2Client.CreateSecurityGroupAsync(createRequest);

            var Groups = new List<string>() { createResponse.Result.GroupId };
            describeRequest = new DescribeSecurityGroupsRequest() { GroupIds = Groups };
            describeResponse = await ec2Client.DescribeSecurityGroupsAsync(describeRequest);
            return  describeResponse.SecurityGroups[0];
        }

       
        
        public async void CreateSecurityGroupIngress(int port, List<IpRange> ranges, SecurityGroup securityGroup)
        {

            var secGroup = securityGroup;
            var ipPermission = new IpPermission()
            {
                IpProtocol = "tcp",
                FromPort = port,
                ToPort = port,
                Ipv4Ranges = ranges,
                //UserIdGroupPairs = new List<UserIdGroupPair>
                //{
                    //new UserIdGroupPair
                    //{

                        //GroupName = "TestSecurityGroup",
                        //GroupId = "TestSecurityGroup"
                    //}
                //}
                //PrefixListIds = new List<PrefixListId>() { new PrefixListId() { Id = "LambdaEntry for automatic IP Whitelisting" } }
                
            };

            
            
        
            //ipPermission.UserIdGroupPairs.Add(new UserIdGroupPair()
            //{
             // GroupId = "Lambda",
              
            //});


            var ingressRequest = new AuthorizeSecurityGroupIngressRequest();
            
            ingressRequest.GroupId = secGroup.GroupId;
            
            
            ingressRequest.IpPermissions.Add(ipPermission);
            

            try
            {
                
                var ingressResponse = await _ec2Client.AuthorizeSecurityGroupIngressAsync(ingressRequest);
                
                Console.WriteLine("New RDP rule for: " + ranges[0]);
            }
            catch (AmazonEC2Exception ex)
            {
                // Check the ErrorCode to see if the rule already exists
                if ("InvalidPermission.Duplicate" == ex.ErrorCode)
                {
                    Console.WriteLine("An RDP rule for: {0} already exists.", ranges[0]);
                }
                else
                {
                    // The exception was thrown for another reason, so re-throw the exception
                    Console.WriteLine(ex.Message);

                }
            }

            

        }

        public async void DeleteSecurityGroupIngress(SecurityGroup secGroup)
        {
            List<IpPermission> ipPermList = new List<IpPermission>();

       

            RevokeSecurityGroupIngressRequest request = new RevokeSecurityGroupIngressRequest()
            {
                GroupId = secGroup.GroupId,
                GroupName = secGroup.GroupName,
                IpPermissions = secGroup.IpPermissions

            };

            await _ec2Client.RevokeSecurityGroupIngressAsync(request);
        }

        public async Task<SecurityGroup> CreateSecurityGroup(AmazonEC2Client _ec2Client, string groupName)
        {
            
            var securityGroup = await CreateEc2SecurityGroup(_ec2Client, groupName);

            return securityGroup;
        }

    }
}
