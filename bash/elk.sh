#!/usr/bin/env bash

## reference: https://logz.io/blog/deploying-the-elk-stack-on-kubernetes-with-helm/

gcloud container clusters get-credentials elk --region us-central1

cat > rbac-config.yaml <<EOF
apiVersion: v1
kind: ServiceAccount
metadata:
  name: tiller
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: tiller
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: cluster-admin
subjects:
  - kind: ServiceAccount
    name: tiller
    namespace: kube-system
EOF

kubectl cluster-info
curl https://raw.githubusercontent.com/kubernetes/Helm/master/scripts/get > get_Helm.sh
chmod 700 get_Helm.sh
helm init
kubectl get pods -n kube-system | grep tiller
helm repo add elastic https://Helm.elastic.co
curl -O https://raw.githubusercontent.com/elastic/Helm-charts/master/elasticsearch/examples/minikube/values.yaml
helm install --name elasticsearch elastic/elasticsearch -f ./values.yaml 
kubectl delete deployment tiller-deploy -n kube-system
kubectl create -f rbac-config.yaml
helm init --service-account tiller --history-max 200 --upgrade
curl -O https://raw.githubusercontent.com/elastic/Helm-charts/master/elasticsearch/examples/minikube/values.yaml
helm install --name elasticsearch elastic/elasticsearch -f ./values.yaml 
kubectl get pods --namespace=default -l app=elasticsearch-master -w
helm install --name kibana elastic/kibana
kubectl get pods -w
helm install --name metricbeat elastic/metricbeat

kubectl get pods -w
kubectl get pods

# two terminal port forward
kubectl port-forward svc/elasticsearch-master 9200
curl localhost:9200/_cat/indices
kubectl port-forward deployment/kibana-kibana 5601 
