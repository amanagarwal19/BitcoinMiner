# Bitcoin Miner

Bitcoin is encrypted using high power super computers that mine a particular string pattern to get a desired hashed output. This project mimics the same ideology by finding strings (coins) which when hashed using SHA256, give the desired number of zeros.

The project distributed the computation needs across the different cores of the machine on which it is run. Then the different cores **concurrently perform their tasks** to increase the efficiency and reduce the time required to mine such coins.

The project successfully achieves a parallelism ratio of 3.4.

### **Scaling the Miner** 

The system has been extended to run across machines **remotely** using TCP connection. This allows **pooling of computation resources** to increase the computational efficiency.

## Steps to run

>1.  `git clone amanagarwal19/BitcoinMiner`
>2.  `dotnet fsi actor.fsx <numberOfZerosNeeded`
