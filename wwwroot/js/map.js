// map.js
export function initMap(apiKey) {
	import(
		`https://maps.googleapis.com/maps/api/js?key=${apiKey}&v=beta&libraries=marker`
	).then(({ AdvancedMarkerElement }) => {
		const map = new google.maps.Map(document.getElementById('map'), {
			center: { lat: 0, lng: 0 },
			zoom: 2,
		});

		if (navigator.geolocation) {
			navigator.geolocation.getCurrentPosition((position) => {
				const pos = {
					lat: position.coords.latitude,
					lng: position.coords.longitude,
				};

				map.setCenter(pos);
				map.setZoom(15);

				new google.maps.Marker({
					position: pos,
					map: map,
					title: 'You are here!',
				});
			});
		} else {
			new google.maps.InfoWindow({
				content: 'Geolocation not supported',
				position: map.getCenter(),
			}).open(map);
		}
	});
}
