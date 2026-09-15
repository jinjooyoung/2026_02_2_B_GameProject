using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("주문 설정")]
    public float orderGenerateInterval = 15f;       // 주문 생성시간
    public int maxActiveOrders = 8;                 // 최대 주문 숫자

    [Header("게임 상태")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    // 주문 리스트
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    // Building 참조
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    // Event 시스템
    [System.Serializable]
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickedUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvents orderEvents;
    public DeliveryDriver driver;

    private void Start()
    {
        driver = FindFirstObjectByType<DeliveryDriver>();
        FindAllBuilding();

        // 초기 주문 생성
        StartCoroutine(GenerateInitialOrders());
        // 주기적 주문 생성
        StartCoroutine(OrderGenerator());
        // 만료 체크
        StartCoroutine(ExpiredOrderChecker());
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 1300));

        GUILayout.Label("=== 배달 주문 ===");
        GUILayout.Label($"활성 주문 : {currentOrders.Count} 개");
        GUILayout.Label($"픽업 대기 : {GetPickWaitingCount()} 개");
        GUILayout.Label($"배달 대기 : {GetDeliveryWaitingCount()} 개");
        GUILayout.Label($"완료 : {completedOrders} 개 | 만료 : {expiredOrders} 개");

        GUILayout.Space(10);

        foreach (DeliveryOrder order in currentOrders)
        {
            string status = order.state == OrderState.WaitingPickup ? "픽업 대기" : "배달 대기";
            float timeLeft = order.GetRamainingTime();

            GUILayout.Label($"#{order.orderId} : {order.restaurantName} -> {order.customerName}");
            GUILayout.Label($"{status} | {timeLeft:F0} 초 남음");
        }

        GUILayout.EndArea();
    }

    void FindAllBuilding()
    {
        Building[] allBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);

        foreach (Building building in allBuildings)
        {
            if (building.buildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if (building.buildingType == BuildingType.Customer)
            {
                customers.Add(building);
            }
        }

        Debug.Log($"음식점 {restaurants.Count} 개, 고객 {customers.Count} 명 발견");
    }

    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        // 랜덤 음식점과 고객 선택
        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCustomer = customers[Random.Range(0, customers.Count)];

        // 같은 건물이면 다시 선택
        if (randomRestaurant == randomCustomer)
        {
            randomCustomer = customers[Random.Range(0, customers.Count)];
        }

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(++totalOrdersGenerated, randomRestaurant, randomCustomer, reward);

        currentOrders.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);
    }

    // 픽업 함수
    void PickUpOrder(DeliveryOrder order)
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickedUp?.Invoke(order);
    }

    // 배달 완료 함수
    void CompleteOrder(DeliveryOrder order)
    {
        order.state = OrderState.Completed;
        completedOrders++;

        // 보상 지급
        if (driver != null)
        {
            driver.AddMoney(order.reward);
        }

        // 완료된 주문 제거
        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    // 주문 취소 소멸
    void ExpireOrder(DeliveryOrder order)
    {
        order.state = OrderState.Exprired;
        expiredOrders++;

        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    // UI 정보 제공
    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.WaitingPickup) count++;
        }
        return count;
    }

    public int GetDeliveryWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.PickedUp) count++;
        }
        return count;
    }

    // 주문 찾는 함수들
    DeliveryOrder FindOrderForPickUp(Building restaurant)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.restaurantBuilding == restaurant && order.state == OrderState.WaitingPickup)
            {
                return order;
            }
        }

        return null;
    }

    DeliveryOrder FindOrderForDelivery(Building customer)
    {
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.customerBuilding == customer && order.state == OrderState.PickedUp)
            {
                return order;
            }
        }

        return null;
    }

    // 주문 처리 함수들
    public void OnDriverEnteredRestaurant(Building restaurant)
    {
        DeliveryOrder orderToPickUp = FindOrderForPickUp(restaurant);

        if (orderToPickUp != null)
        {
            PickUpOrder(orderToPickUp);
        }
    }

    public void OnDriverEnteredCustom(Building customer)
    {
        DeliveryOrder orderToDelivery = FindOrderForDelivery(customer);

        if (orderToDelivery != null)
        {
            CompleteOrder(orderToDelivery);
        }
    }

    // 주문 생성 코루틴
    IEnumerator GenerateInitialOrders()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            CreateNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator OrderGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(orderGenerateInterval);

            if (currentOrders.Count < maxActiveOrders)
            {
                CreateNewOrder();
            }
        }
    }

    // 만료 코루틴 함수
    IEnumerator ExpiredOrderChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            List<DeliveryOrder> expiredOrders = new List<DeliveryOrder>();

            foreach (DeliveryOrder order in currentOrders)
            {
                if (order.IsExprired() && order.state != OrderState.Completed)
                {
                    expiredOrders.Add(order);
                }
            }

            foreach (DeliveryOrder expired in expiredOrders)
            {
                ExpireOrder(expired);
            }
        }
    }
}
