# Simple monte-carlo simulation how development cycle time is affected by testing strategy

Each run of simulation consists of these steps:

* Development
* Per-commit automated tests
* Daily automted tests
* Manual tests

Whole process represents a feature being implemented and tested before being declared as done.
Feature starts in development, where time is spent. There is a chance that error is introduced. Then, each consecutive testing stage follows. Each has time of how long it takes and a chance of finding an error, if it exists. If error is found, the process goes back to development, but this time, the fix time is shorter and chance of introducing an error is lower. Probability of each testing stage is "what is chance that error is found if previous stage didn't find any". The process ends when all testing stages pass without finding an error.

Output of each simulation is how long the whole process went and if there is still an error even when going through all the testing stages. Those results are aggregated when thousands of simulations are run.

## Results of some simulations

### Minimal testing
Development process with minimal testing. Notice high amount of errors that get through.
* Missed errors probability: 36.6%
* Development time average: 17.8 h
* 50 percentile : 13.8 h
* 70 percentile : 18.7 h
* 85 percentile : 26.3 h
* 95 percentile : 41.2 h
### Heavy manual testing
Process where most testing is done manually. Much slower, but less errors.
* Missed errors probability: 15.2%
* Development time average: 35.9 h
* 50 percentile : 31.4 h
* 70 percentile : 41.7 h
* 85 percentile : 53.8 h
* 95 percentile : 71.8 h
### Mostly integration tests
Testing where slow integration tests are used. Allows for lower manual testing. Faster and less errors.
* Missed errors probability: 9.4%
* Development time average: 24.7 h
* 50 percentile : 21.4 h
* 70 percentile : 28.2 h
* 85 percentile : 36.5 h
* 95 percentile : 50.3 h
### Unit + integration tests
Testing with some unit tests and integration tests. Allows for low manual testing. Faster and less errors.
* Missed errors probability: 6.3%
* Development time average: 18.9 h
* 50 percentile : 15.8 h
* 70 percentile : 20.7 h
* 85 percentile : 27.7 h
* 95 percentile : 41.1 h
### Better unit + integration tests
Testing with good unit tests and integration tests. Allows for minimal manual testing. Even faster and less errors.
* Missed errors probability: 2.0%
* Development time average: 15.1 h
* 50 percentile : 12.2 h
* 70 percentile : 16.1 h
* 85 percentile : 21.8 h
* 95 percentile : 33.1 h
### Continuous delivery
Testing with highly reliable unit tests and integration tests and almost no manual testing. Really fast and almost no errors.
* Missed errors probability: 0.4%
* Development time average: 13.9 h
* 50 percentile : 11.0 h
* 70 percentile : 14.6 h
* 85 percentile : 20.3 h
* 95 percentile : 31.5 h
### Heavy manual testing + shorter dev time
Same as Heavy manual testing but with shorter development times. Development speed has minimal impact when testing is slow.
* Missed errors probability: 14.5%
* Development time average: 29.8 h
* 50 percentile : 25.8 h
* 70 percentile : 34.9 h
* 85 percentile : 45.6 h
* 95 percentile : 60.6 h
### Continuous delivery + shorter dev time
Same as Continuous delivery but with shorter development times. Fast development has huge impact when testing is mostly automated.
* Missed errors probability: 0.7%
* Development time average: 7.5 h
* 50 percentile : 7.0 h
* 70 percentile : 8.2 h
* 85 percentile : 10.1 h
* 95 percentile : 13.0 h