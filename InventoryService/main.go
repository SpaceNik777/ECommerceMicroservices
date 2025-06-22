package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"strconv"
	"sync"
	"time"

	"github.com/Shopify/sarama"
)

type Product struct {
	ID       string `json:"id"`
	Quantity int    `json:"quantity"`
}

type UpdateStockRequest struct {
	ProductID string `json:"productId"`
	Quantity  int    `json:"quantity"`
}

type HealthResponse struct {
	Status    string `json:"status"`
	Timestamp string `json:"timestamp"`
}

var (
	products = make(map[string]Product)
	mu       sync.RWMutex
)

func waitForKafka(brokers []string, maxRetries int) error {
	for i := 0; i < maxRetries; i++ {
		config := sarama.NewConfig()
		config.Version = sarama.V2_8_0_0
		config.Consumer.Return.Errors = true

		client, err := sarama.NewClient(brokers, config)
		if err == nil {
			client.Close()
			return nil
		}

		log.Printf("Waiting for Kafka to be ready... (attempt %d/%d)", i+1, maxRetries)
		time.Sleep(5 * time.Second)
	}
	return fmt.Errorf("failed to connect to Kafka after %d attempts", maxRetries)
}

func healthCheck(w http.ResponseWriter, r *http.Request) {
	response := HealthResponse{
		Status:    "healthy",
		Timestamp: time.Now().Format(time.RFC3339),
	}
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

func main() {
	// Получение порта из переменных окружения
	port := getEnv("PORT", "8082")

	// Получение настроек Kafka
	kafkaBrokers := getEnv("KAFKA_BROKERS", "kafka:9092")
	brokers := []string{kafkaBrokers}

	// Ожидание доступности Kafka
	if err := waitForKafka(brokers, 12); err != nil {
		log.Fatalf("Failed to connect to Kafka: %v", err)
	}

	// Создание и запуск обработчика событий
	eventHandler, err := NewEventHandler(brokers)
	if err != nil {
		log.Fatalf("Failed to create event handler: %v", err)
	}

	// Запуск обработчика событий в отдельной горутине
	go func() {
		if err := eventHandler.Start(); err != nil {
			log.Printf("Event handler error: %v", err)
		}
	}()

	// Регистрация HTTP обработчиков
	http.HandleFunc("/health", healthCheck)
	http.HandleFunc("/api/inventory/check", checkAvailability)
	http.HandleFunc("/api/inventory/quantity", getProductQuantity)
	http.HandleFunc("/api/inventory/update", updateStock)

	// Запуск HTTP сервера
	log.Printf("Starting server on port %s", port)
	if err := http.ListenAndServe(":"+port, nil); err != nil {
		log.Fatalf("Failed to start server: %v", err)
	}
}

func getEnv(key, defaultValue string) string {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}
	return value
}

func checkAvailability(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	productID := r.URL.Query().Get("productId")
	quantityStr := r.URL.Query().Get("quantity")

	if productID == "" || quantityStr == "" {
		http.Error(w, "Product ID and quantity are required", http.StatusBadRequest)
		return
	}

	quantity, err := strconv.Atoi(quantityStr)
	if err != nil {
		http.Error(w, "Invalid quantity", http.StatusBadRequest)
		return
	}

	mu.RLock()
	product, exists := products[productID]
	mu.RUnlock()

	if !exists {
		http.Error(w, "Product not found", http.StatusNotFound)
		return
	}

	if product.Quantity < quantity {
		http.Error(w, "Insufficient stock", http.StatusBadRequest)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]bool{"available": true})
}

func getProductQuantity(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	productID := r.URL.Query().Get("productId")

	if productID == "" {
		http.Error(w, "Product ID is required", http.StatusBadRequest)
		return
	}

	mu.RLock()
	product, exists := products[productID]
	mu.RUnlock()

	if !exists {
		http.Error(w, "Product not found", http.StatusNotFound)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]int{"quantity": product.Quantity})
}

func updateStock(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	var req UpdateStockRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	mu.Lock()
	defer mu.Unlock()

	product, exists := products[req.ProductID]
	if !exists {
		product = Product{ID: req.ProductID, Quantity: 0}
	}

	product.Quantity += req.Quantity
	if product.Quantity < 0 {
		http.Error(w, "Insufficient stock", http.StatusBadRequest)
		return
	}

	products[req.ProductID] = product
	w.WriteHeader(http.StatusOK)
}
