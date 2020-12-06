# Home assistant integration

This is my own integration of the pool controller into Home Assistant.
Please visit [Home assistant web site](https://www.home-assistant.io/) for complete presentation of home assistant.

## Integrate the pool controller

Declaration of my remote pool controller using REST commands.

```yml
sensor:
    # Read all the statuses in 1 http REST query (every 20 seconds)
  - platform: rest
    name: swimming_pool_rpi
    resource: http://192.xxx.xxx.xxx/api/v1/status
    json_attributes:
      - status
      - waterTemperature
      - poolTemperature
      - airTemperature
      - poolTemperatureMinOfTheDay
      - poolTemperatureMaxOfTheDay
      - poolTemperatureDecision
      - pumpingDurationPerDayInHours
      - pump
      - pumpForceOff
      - pumpForceOn
      - chlorineInhibition
      - phRegulationInhibition
      - watering
      - swimmingPoolLight
      - deckLight
    value_template: "{{ value_json.pump }}"
    scan_interval: 20

rest_command:
  swimming_pool_rpi_switch_on:
    url: http://192.xxx.xxx.xxx/api/v1/io/{{ switch_name }}
    method: POST
    payload: '{"active": true}'
    content_type: application/json
    verify_ssl: false

  swimming_pool_rpi_switch_off:
    url: http://192.xxx.xxx.xxx/api/v1/io/{{ switch_name }}
    method: POST
    payload: '{"active": false}'
    content_type: application/json
    verify_ssl: false

  swimming_pool_rpi_action:
    url: http://192.xxx.xxx.xxx/api/v1/action/{{ action_name }}
    method: POST
    content_type: application/json
    verify_ssl: false

```

## Declare sensors


```yml
sensor:
    # Read values from previous REST query.
  - platform: template
    sensors:
      outdoor_temperature:
        friendly_name: Outdoor temperature
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["airTemperature"] }}'
        unit_of_measurement: "°C"
        device_class: temperature

      swimming_pool_water_pipe_temperature:
        friendly_name: Pipe temperature
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["waterTemperature"] }}'
        unit_of_measurement: "°C"
        device_class: temperature

      swimming_pool_pool_temperature:
        friendly_name: Pool temperature
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["poolTemperature"] }}'
        unit_of_measurement: "°C"
        device_class: temperature

      swimming_pool_temperature_setpoint:
        friendly_name: Set point temperature
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["poolTemperatureDecision"] }}'
        unit_of_measurement: "°C"
        device_class: temperature

      swimming_pool_pumping_duration_per_day:
        friendly_name: Pumping duration per day (h)
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["pumpingDurationPerDayInHours"] }}'
        unit_of_measurement: "h"
```

## Declare switches

```yml
homeassistant:
  customize:
    switch.swimming_deck_light:
      icon: mdi:lightbulb-outline

    switch.swimming_pool_light:
      icon: mdi:lightbulb-outline


binary_sensor:
  - platform: template
    sensors:
      swimming_pool_pump_run:
        friendly_name: Pump
        device_class: power
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["pump"] }}'

      swimming_pool_disable_ph_treatment:
        friendly_name: Disable PH treatment
        device_class: power
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["phRegulationInhibition"] }}'

switch:
  - platform: template
    switches:
      swimming_deck_light:
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["deckLight"] }}'
        turn_on:
          - service: rest_command.swimming_pool_rpi_switch_on
            data:
              switch_name: DeckLight
        turn_off:
          - service: rest_command.swimming_pool_rpi_switch_off
            data:
              switch_name: DeckLight
        friendly_name: Deck light

      swimming_pool_light:
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["swimmingPoolLight"] }}'
        turn_on:
          - service: rest_command.swimming_pool_rpi_switch_on
            data:
              switch_name: swimmingPoolLight
        turn_off:
          - service: rest_command.swimming_pool_rpi_switch_off
            data:
              switch_name: swimmingPoolLight
        friendly_name: Pool light

      swimming_pool_pump_manual_force_on:
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["pumpForceOn"] }}'
        turn_on:
          - service: rest_command.swimming_pool_rpi_switch_on
            data:
              switch_name: PumpForceOn
        turn_off:
          - service: rest_command.swimming_pool_rpi_switch_off
            data:
              switch_name: PumpForceOn
        friendly_name: Pump - Manual ON

      swimming_pool_pump_manual_force_off:
        value_template: '{{ states.sensor.swimming_pool_rpi.attributes["pumpForceOff"] }}'
        turn_on:
          - service: rest_command.swimming_pool_rpi_switch_on
            data:
              switch_name: PumpForceOff
        turn_off:
          - service: rest_command.swimming_pool_rpi_switch_off
            data:
              switch_name: PumpForceOff
        friendly_name: Pump - Manual OFF

```

## Declare covers

There is no position feedback of the covers (`position_template: "{{ 50 }}"`)

```yml
cover:
  - platform: template
    covers:
      swimming_pool_cover:
        friendly_name: "Pool cover"
        position_template: "{{ 50 }}"
        open_cover:
          - service: rest_command.swimming_pool_rpi_action
            data:
              action_name: OpenCover
        close_cover:
          - service: rest_command.swimming_pool_rpi_action
            data:
              action_name: CloseCover
        stop_cover:
          - service: rest_command.swimming_pool_rpi_action
            data:
              action_name: StopCover
```




## Automations and manual forcings

Allow to force ON or OFF the pumping system during a specified amount of time.

```yml
input_number:
  # Use either to force ON or OFF the pump during the specified duration
  swimming_pool_pump_manual_duration:
    name: Manual forcing duration (h)
    initial: 2
    min: 1
    max: 36
    step: 1
    mode: box

automation:
  - alias: Switch off lights at 3h00AM
    initial_state: true
    trigger:
      platform: time
      at: "03:00:00"
    action:
      - service: switch.turn_off
        entity_id: switch.swimming_deck_light
      - service: switch.turn_off
        entity_id: switch.swimming_pool_light

  - alias: Pool - Disable manual ON forcing
    initial_state: true
    trigger:
      platform: state
      entity_id: switch.swimming_pool_pump_manual_force_on
      to: "on"
      for:
        hours: "{{ states('input_number.swimming_pool_pump_manual_duration')|int }}"
    action:
      service: switch.turn_off
      entity_id: switch.swimming_pool_pump_manual_force_on

  - alias: Pool - Disable manual OFF forcing
    initial_state: true
    trigger:
      platform: state
      entity_id: switch.swimming_pool_pump_manual_force_off
      to: "on"
      for:
        hours: "{{ states('input_number.swimming_pool_pump_manual_duration')|int }}"
    action:
      service: switch.turn_off
      entity_id: switch.swimming_pool_pump_manual_force_off

```
