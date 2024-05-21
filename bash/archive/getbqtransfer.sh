PROJECT_NUMBER="268323096258"

bq mk \
--transfer_config \
--project_id=project_id \
--target_dataset=dataset \
--display_name=name \
--params='parameters' \
--data_source=data_source

bq mk \
--transfer_run \
--start_time='start_time' \
--end_time='end_time' \
resource_name

bq ls --transfer_config --transfer_location=US

bq show --format=prettyjson --transfer_config projects/${PROJECT_NUMBER}/locations/us/transferConfigs/61879649-0000-27ae-abd5-94eb2c042b1c
