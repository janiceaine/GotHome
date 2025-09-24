// map.js
let map; // Global map reference
let markers = []; // Store all markers

// Initialize the map
function initMap() {
	console.log('initMap called');
	// Access API key and Map ID if needed
	const apiKey = window.mapConfig?.apiKey || '';
	const mapId = window.mapConfig?.mapId || 'DEMO_MAP_ID'; // Fallback to DEMO_MAP_ID for testing
	console.log('API Key in map.js:', apiKey);
	console.log('Map ID in map.js:', mapId);

	// Initialize map with default center and Map ID
	map = new google.maps.Map(document.getElementById('map'), {
		center: { lat: 0, lng: 0 },
		zoom: 2,
		mapId: mapId, // Uses the Map ID from secrets/config
	});

	// Add current location marker using geolocation
	if (navigator.geolocation) {
		navigator.geolocation.getCurrentPosition(
			(position) => {
				const pos = {
					lat: position.coords.latitude,
					lng: position.coords.longitude,
				};
				console.log('Geolocation success, centering map at:', pos);
				map.setCenter(pos);
				map.setZoom(15);
				addMarker(pos.lat, pos.lng, 'blue', 'Current Location');
				// Dispatch event to signal map is initialized
				window.dispatchEvent(new Event('mapInitialized'));
			},
			(error) => {
				console.error('Geolocation error:', error);
				// Fallback: Initialize map and dispatch event
				window.dispatchEvent(new Event('mapInitialized'));
			}
		);
	} else {
		console.error('Geolocation not supported');
		window.dispatchEvent(new Event('mapInitialized'));
	}
}

// Make initMap globally accessible for Google Maps API callback
window.initMap = initMap;

// Add a marker using AdvancedMarkerElement
export function addMarker(lat, lng, color = 'red', title = 'location') {
	if (!map) {
		console.error('Map not initialized');
		return;
	}

	// Validate lat/lng
	if (
		typeof lat !== 'number' ||
		typeof lng !== 'number' ||
		isNaN(lat) ||
		isNaN(lng)
	) {
		console.error('Invalid coordinates:', { lat, lng });
		return;
	}

	console.log('Adding marker:', { lat, lng, color, title });

	// Create a PinElement for the marker's appearance
	const pin = new google.maps.marker.PinElement({
		background: color,
		borderColor: 'white',
		glyphColor: 'white',
		scale: 1.0,
	});

	// Create a DOM element for the label
	const labelElement = document.createElement('div');
	labelElement.style.color = 'black';
	labelElement.style.fontSize = '14px';
	labelElement.style.fontWeight = 'bold';
	labelElement.style.position = 'absolute';
	labelElement.style.transform = 'translate(0, 20px)'; // Position below marker
	labelElement.textContent = title;

	// Create the AdvancedMarkerElement
	const marker = new google.maps.marker.AdvancedMarkerElement({
		map: map,
		position: { lat, lng },
		content: pin.element,
		title: title,
	});

	// Append the label to the marker
	marker.element.appendChild(labelElement);

	markers.push(marker);
	return marker;
}
