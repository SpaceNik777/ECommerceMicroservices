package main

import (
	"context"
	"encoding/json"
	"log"
	"os"
	"os/signal"
	"syscall"

	"github.com/Shopify/sarama"
)

// OrderEvent представляет структуру события заказа
type OrderEvent struct {
	OrderID    string      `json:"orderId"`
	Items      []OrderItem `json:"items"`
	TotalPrice float64     `json:"totalPrice"`
}

// OrderItem представляет структуру элемента заказа
type OrderItem struct {
	ProductID string  `json:"productId"`
	Quantity  int     `json:"quantity"`
	Price     float64 `json:"price"`
}

// EventHandler обрабатывает события Kafka
type EventHandler struct {
	consumer sarama.Consumer
	topics   []string
}

// NewEventHandler создает новый обработчик событий
func NewEventHandler(brokers []string) (*EventHandler, error) {
	config := sarama.NewConfig()
	config.Version = sarama.V2_8_0_0
	config.Consumer.Return.Errors = true
	config.Consumer.Offsets.Initial = sarama.OffsetOldest

	consumer, err := sarama.NewConsumer(brokers, config)
	if err != nil {
		return nil, err
	}

	return &EventHandler{
		consumer: consumer,
		topics:   []string{"order-placed"},
	}, nil
}

// Start начинает обработку событий
func (h *EventHandler) Start() error {
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Обработка сигналов для graceful shutdown
	signals := make(chan os.Signal, 1)
	signal.Notify(signals, syscall.SIGINT, syscall.SIGTERM)

	// Запуск обработки сообщений для каждого топика
	for _, topic := range h.topics {
		partitionConsumer, err := h.consumer.ConsumePartition(topic, 0, sarama.OffsetOldest)
		if err != nil {
			return err
		}

		go func(pc sarama.PartitionConsumer) {
			defer pc.Close()
			for {
				select {
				case msg := <-pc.Messages():
					if err := h.handleMessage(msg); err != nil {
						log.Printf("Error handling message: %v", err)
					}
				case err := <-pc.Errors():
					log.Printf("Error consuming message: %v", err)
				case <-ctx.Done():
					return
				}
			}
		}(partitionConsumer)
	}

	// Ожидание сигнала завершения
	<-signals
	return nil
}

// handleMessage обрабатывает сообщение из Kafka
func (h *EventHandler) handleMessage(msg *sarama.ConsumerMessage) error {
	var event OrderEvent
	if err := json.Unmarshal(msg.Value, &event); err != nil {
		return err
	}

	log.Printf("Received order event: %+v", event)

	// Обновление запасов для каждого товара в заказе
	for _, item := range event.Items {
		mu.Lock()
		product, exists := products[item.ProductID]
		if !exists {
			product = Product{
				ID:       item.ProductID,
				Quantity: 0,
			}
		}

		// Проверка на отрицательный остаток
		if product.Quantity < item.Quantity {
			log.Printf("Warning: Insufficient stock for product %s", item.ProductID)
			mu.Unlock()
			continue
		}

		product.Quantity -= item.Quantity
		products[item.ProductID] = product
		mu.Unlock()

		log.Printf("Updated stock for product %s: %d", item.ProductID, product.Quantity)
	}

	return nil
}

// Close закрывает обработчик событий
func (h *EventHandler) Close() error {
	return h.consumer.Close()
}
