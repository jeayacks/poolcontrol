# Raspberry pool control

The Rapsberry solution which automates your swimming pool !

Manages pump cycles depending on water temperature and protect your swimming pool during the winter against frost.

Web APIs enable integration with your favorite home automation system.

![.NET Core](https://github.com/bluewaterwise/poolcontrol/workflows/.NET%20Core/badge.svg)

Example of integration in [Home assistant](https://www.home-assistant.io/):

![Home assistant integration](doc/medias/home-assistant-screenshot.png)

## The context

Because for me swimming is a pleasure, I don't want to spend a lot of time for setting up the water filtration and the treatements, minimum is the best :-).
The initial goal is to automaticaly adjust the pumping duration according the water temperature with few modifications of the existing system.

![Physical system](doc/medias/swimming-pool-installation.svg)

The unique physical change consits in adding a temperature sensor (ds18b20) directly on the filtration system pipe. This temperature sensor measures the pool temperature after a significant pumping duration (usualy 15 mins), and it is used during winter period as direct measure to switch on the pump in case of frost conditions.

For your convenience and if you don't want to upgrade your installation you can use any other sensor temperature like RF433 swimming temperature sensor directly immersed in the water.

## The hardware

The system is using a Raspberry PI 3 with an 8 relays board and 2 ds18b20 sensors for temperature.

* Raspberry PI3 (or PI4), I used PI 3 B+ for this project.

* Case for raspberry on DIN Rail ([Amazon](https://www.amazon.com/GeeekPi-Case-Raspberry-Pi4-Rail/dp/B083916S3S/ref=sr_1_2?dchild=1&keywords=raspberry+din+rail+case&qid=1601238952&sr=8-2))

* 12v power supply on DIN Rail ([Amazon](https://www.amazon.com/MEAN-WELL-DR-15-5-DIN-Rail-Supply/dp/B005T6L33I/ref=sr_1_10?dchild=1&keywords=raspberry+pi+12v+power+supply+din+rail&qid=1601239065&sr=8-10))

* 5V 8-Channel Relay interface board

* 2 x ds18b20

The system controls : 

* The pool pump

* The PH regulator

* The pool cover

* The pool light

* The deck light

* The watering system of the garden

## The pool electrical part

![Physical system](doc/medias/electric-wire.svg)

As shown on the diagram, all added components allow the legacy system to continue working if the Raspberry controller fails, it just need to reactivate the electronic clock to pilot the pump.
The second added relay controls the PH Regulator, the regulator don't need to stay on during all the pumping cycle this is why it is controlled by the system, and for the same backup reasons, if Raspberry is down, the legacy system continue working normally.

## The control logic

## Install and configure on raspberry

## 


Only 2 scenarios are possible : Winter and Summer and total pumping time is calculated using the temperature.

The proposal is to introduce a intermediate group of pump cycles depending on temperature range

Current solution
```json
{
  "SummerPumpingCycles": [
    {
      "DecisionTime": "02:00:00"
    },
    {
      "DecisionTime": "09:00:00"
    }
  ]
}

```


Proposal :
```json
{
  "SummerPumpingCycles":[
      {
        "name": "default cycles",
        "range": "default",
        "cycles":[
            {
            "DecisionTime": "02:00:00"
            },
            {
            "DecisionTime": "09:00:00"
            }
        ]
      },
      {
        "name": "Lower than 20°",
        "range": "0-20",
        "cycles":[
            {
            "DecisionTime": "02:00:00"
            },
            {
            "DecisionTime": "09:00:00"
            }
        ]
      }
  ]
  }
}
```
