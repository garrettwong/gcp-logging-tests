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

bq show --format=prettyjson --transfer_config projects/268323096258/locations/us/transferConfigs/61879649-0000-27ae-abd5-94eb2c042b1c

# {
#   "dataSourceId": "6063d10f-0000-2c12-a706-f403045e6250",
#   "datasetRegion": "us",
#   "destinationDatasetId": "recommender",
#   "displayName": "recommender",
#   "emailPreferences": {
#     "enableFailureEmail": true
#   },
#   "name": "projects/268323096258/locations/us/transferConfigs/61879649-0000-27ae-abd5-94eb2c042b1c",
#   "nextRunTime": "2021-08-03T00:00:00Z",
#   "notificationPubsubTopic": "projects/gwc-sandbox/topics/recommender",
#   "params": {
#     "organization_id": "358329783625"
#   },
#   "schedule": "every day 04:00",
#   "state": "RUNNING",
#   "updateTime": "2021-08-02T04:01:56.952463Z",
#   "userId": "672100267182682579"
# }
