using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;


// Add using statements to access AWS SDK for .NET services. 
// Both the Service and its Model namespace need to be added 
// in order to gain access to a service. For example, to access
// the EC2 service, add:
using Amazon.EC2;
using Amazon.EC2.Model;

namespace AwsEc2
{
    class Program
    {
        private const string INSTANCE_TAG = "craig";

        private static IAmazonEC2 ec2;
        private static Instance instance;
        private static int instanceCount;

        public static void Main(string[] args)
        {
            Initialize();

            Console.WriteLine("===========================================");
            Console.WriteLine("         Alans IN720 AWS assignment        ");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("Please enter one of the following commands : start, stop, status, terminate ");
            Console.WriteLine();

            String userInput = Console.ReadLine();

            switch (userInput)
            {
                case "start":                                           //  1. Start or create an instance
                    StartOrCreate();
                    break;
                case "stop":                                            //  2. Stop an instance
                    Stop();
                    break;
                case "status":                                          //  3.  Prints the status of an instance
                    Status();
                    break;
                case "terminate":                                       //  4.  Terminates a status
                    Terminate();
                    break;
                default:                                                //  If user inputs invalid argument
                    // Deal with wrong input
                    break;
            }
            
            Console.Read();
            

            // TRY & CATCH !!

        }
        // Method to start an instance if it exists else will call create
        private static void StartOrCreate()
        {
            if (instance != null)
            {
                Console.WriteLine("Starting instance...");
                StartInstancesRequest request = new StartInstancesRequest();
                request.InstanceIds.Add(instance.InstanceId);
                ec2.StartInstances(request);
                Console.WriteLine("Successfully started instance {0}", instance.InstanceId);
            }
            else Create();
            
        }
        // Method to create an instance
        private static void Create()
        {
            Console.WriteLine("Creating instance...");

            string amiID = "ami-48db9d28";
            string keyPairName = "ec2craia4";
            string securityGroup = "launch-wizard-2-craia4";

            RunInstancesRequest request = new RunInstancesRequest();
            request.ImageId = amiID;
            request.InstanceType = "t2.micro";
            request.MinCount = 1;
            request.MaxCount = 1;
            request.KeyName = keyPairName;
            request.SecurityGroupIds.Add(securityGroup);
            //RunInstancesResponse response = ec2.RunInstances(request);
            ec2.RunInstances(request);
            
            DescribeInstancesResponse result = ec2.DescribeInstances(new DescribeInstancesRequest());

            List<Reservation> reservations = result.Reservations;
            instance = reservations[0].Instances[0];
            Tag tag = new Tag("Name", INSTANCE_TAG);

            CreateTagsRequest tagRequest = new CreateTagsRequest();
            tagRequest.Resources.Add(instance.InstanceId);
            tagRequest.Tags.Add(tag);

            ec2.CreateTags(tagRequest);

            Console.WriteLine("Successfully created instance {0}", instance.InstanceId);
        }
        // Method to stop an instance
        private static void Stop()
        {
            Console.WriteLine("Stopping instance...");
            StopInstancesRequest request = new StopInstancesRequest();
            request.InstanceIds.Add(instance.InstanceId);
            ec2.StopInstances(request);
            Console.WriteLine("Successfully stopped instance {0}", instance.InstanceId);
        }
        // Method to print the status of an instance
        private static void Status()
        {
            DescribeInstanceStatusRequest statusRequest = new DescribeInstanceStatusRequest();
            statusRequest.InstanceIds.Add(instance.InstanceId);
            InstanceStatus result = ec2.DescribeInstanceStatus(statusRequest).InstanceStatuses.First();
            Console.WriteLine(result.Status.Status.Value);
        }
        // Method to terminate an instance
        private static void Terminate()
        {
            Console.WriteLine("Terminating instance...");
            TerminateInstancesRequest request = new TerminateInstancesRequest();
            request.InstanceIds.Add(instance.InstanceId);
            ec2.TerminateInstances(request);
            Console.WriteLine("Successfully terminated instance {0}", instance.InstanceId);
        }
        // Initialize vairables
        private static void Initialize()
        {
            ec2 = new AmazonEC2Client();

            DescribeInstancesRequest ec2Request = new DescribeInstancesRequest();

            Filter tagFilter = new Filter();
            tagFilter.Name = "tag:Name";
            tagFilter.Values.Add(INSTANCE_TAG);
            ec2Request.Filters.Add(tagFilter);

            DescribeInstancesResponse result = ec2.DescribeInstances(ec2Request);

            List<Reservation> reservations = result.Reservations;

            instanceCount = reservations.Count;

            if (instanceCount == 1)
            {
                instance = reservations[0].Instances[0];
            }
            else
            {
                instance = null;
            }

        }


    }
}