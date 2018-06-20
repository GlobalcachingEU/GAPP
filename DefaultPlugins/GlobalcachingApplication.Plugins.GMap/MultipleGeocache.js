var map;
var markers = [];
var selectedmarkers = [];
var curposMarker;
var wptmarkers = [];
var markerClusterer = null;
var ignoreCenter = false;
var custonOverlays = [];
var geocoder;
var activeInfoWindow = null;
var clusterOptions = { gridSize: 40, maxZoom: 13, imagePath: 'https://www.4geocaching.eu/modules/Globalcaching/media/m' };
var enableClusterMarkerAboveCount = 1000;
var activeInfoWindow;

//enableClusterMarkerAboveCount
//clusterOptions.maxZoom
//clusterOptions.gridSize

//icons


function init() {
	/*
    Build list of map types.
    You can also use var mapTypeIds = ["roadmap", "satellite", "hybrid", "terrain", "OSM"]
    but static lists sucks when google updates the default list of map types.
    */
    var mapTypeIds = [];
    for(var type in google.maps.MapTypeId) {
        mapTypeIds.push(google.maps.MapTypeId[type]);
    }
    mapTypeIds.push("OSM");
    mapTypeIds.push("Offline");

    var myOptions = {
        zoom: 13,
        center: new google.maps.LatLng(0.0, 0.0),
		scaleControl: true,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
		mapTypeControlOptions: { mapTypeIds: mapTypeIds }
    };
    map = new google.maps.Map(document.getElementById("map"), myOptions);

	//Define OSM map type pointing at the OpenStreetMap tile server
    map.mapTypes.set("OSM", new google.maps.ImageMapType({
        getTileUrl: function(coord, zoom) {
            return "http://tile.openstreetmap.org/" + zoom + "/" + coord.x + "/" + coord.y + ".png";
        },
        tileSize: new google.maps.Size(256, 256),
        name: "OpenStreetMap",
        maxZoom: 18
    }));

    map.mapTypes.set("Offline", new google.maps.ImageMapType({
        getTileUrl: function(coord, zoom) {
            return "http://localhost:6231/" + zoom + "/" + coord.x + "/" + coord.y + ".png";
        },
        tileSize: new google.maps.Size(256, 256),
        name: "Offline",
        maxZoom: 18
    }));

	activeInfoWindow = new google.maps.InfoWindow();

	geocoder = new google.maps.Geocoder();
    map.enableKeyDragZoom({
        visualEnabled: false,
        visualPosition: google.maps.ControlPosition.LEFT,
        visualPositionOffset: new google.maps.Size(35, 0),
        visualPositionIndex: null,
        visualSprite: "http://maps.gstatic.com/mapfiles/ftr/controls/dragzoom_btn.png",
        visualSize: new google.maps.Size(20, 20),
        visualTips: {
            off: "Turn on",
            on: "Turn off"
        }
    });

    curposMarker = new google.maps.Marker({ title: 'none', 'map': map, draggable: false, icon: curposIcon, position: map.getCenter(), flat: true, visible: false });
	bound.pageReady();
}

function setMapCenter(lat, lon) {
		map.setCenter(new google.maps.LatLng(lat, lon));
}

function setCurrentPosition(valid, lat, lon) {
	if (valid) {
		curposMarker.setPosition(new google.maps.LatLng(lat, lon));
		curposMarker.setVisible(true);
	}
	else {
		curposMarker.setVisible(false);
	}
}

function updateWaypoints(wpList) {
	for (var i = 0; i < wptmarkers.length; i++) {
		wptmarkers[i].setMap(null);
	}
	wptmarkers.length = 0;
	var wps = wpList;
	for (var i = 0; i < wps.length; i++) {
		wptmarkers.push(createWaypoint(wps[i].a,new google.maps.LatLng(wps[i].b, wps[i].c), eval(wps[i].d), wps[i].e));
	}
}

function createWaypoint(id,point,ic,balloonCnt) {
   var marker = new google.maps.Marker({ 'title': id, map: map, draggable: false, icon: ic, position: point, flat: false, visible: true });
   var iw = new google.maps.InfoWindow();
   iw.setContent(balloonCnt);
   google.maps.event.addListener(marker, 'click', function () {
   if (activeInfoWindow!=null) activeInfoWindow.close();
   activeInfoWindow = iw;
   iw.open(map, marker);
   });
   return marker;
}

function setSelectedGeocaches(gcList) {
	var wps = gcList;
	for (var i = 0; i < selectedmarkers.length; i++) {
		selectedmarkers[i].setMap(null);
	}
	selectedmarkers.length=0;
	if (wps.length==markers.length) {
		for (var i = 0; i < wps.length; i++) {
			if (wps[i]==1) {
				var marker = new google.maps.Marker({
					position: markers[i].getPosition(),
					icon: selectedIcon,
					zIndex: -1,
					map: map
				});
				selectedmarkers.push(marker);
			}
		}
	}
}

function updateGeocaches(gcList) {
    if (markerClusterer != null) {
        markerClusterer.clearMarkers();
    }
	var wps = gcList;
	markers.length = 0;
	for (var i = 0; i < wps.length; i++) {
		var marker = new google.maps.Marker({
			position: new google.maps.LatLng(wps[i].b, wps[i].c),
			icon: eval(wps[i].d),
			title: wps[i].a
		});
		addClickListener(marker);
		markers.push(marker);
	}
	if (markers.length>=enableClusterMarkerAboveCount)
	{
		markerClusterer = new MarkerClusterer(map, markers, clusterOptions);
	}
	else
	{
		for (var i=0; i<markers.length; i++)
		{
			markers[i].setMap(map);
		}
	}
}

function addClickListener(marker) {
	var gccode = marker.getTitle();
	google.maps.event.addListener(marker, 'click', function () {
		ignoreCenter = true;
		bound.geocacheClicked(gccode);
	});
}


function setGeocache(cacheType, gccode, name, lat, lon, gcic) {
    if (gccode.toString().length > 0) {
        $("#title").html('<img src="' + cacheType + '" />' + gccode + ': ' + name);
		activeInfoWindow.setContent('<img src="' + cacheType + '" />' + gccode + ': ' + name);
		activeInfoWindow.setPosition(new google.maps.LatLng(lat, lon));
		activeInfoWindow.open(map);
    }
    else {
        $("#title").html('');
    }
	if (ignoreCenter)
	{
		ignoreCenter = false;
	}
	else
	{
		map.setCenter(new google.maps.LatLng(lat, lon));
	}
}

function addPolygons(polys) {
    var ps = polys;
    for (var i=0; i<ps.length; i++) {
        createArea(ps[i]);
    }
}

function zoomToBounds(minlat, minlon, maxlat, maxlon) {
    map.fitBounds(new google.maps.LatLngBounds(new google.maps.LatLng(minlat, minlon), new google.maps.LatLng(maxlat, maxlon)));
}

function createArea(area) {
    var points = [];
    for (var i = 0; i < area.points.length; i++) {
        points.push(new google.maps.LatLng(area.points[i].lat, area.points[i].lon));
    }
    custonOverlays.push(new google.maps.Polygon({ clickable: false, paths: points, map: map, fillOpacity: 0.1 }));
}

function setCenter(lat, lon) {
    map.setCenter(new google.maps.LatLng(lat, lon));
}

function showAddress(address) {
    geocoder.geocode({ 'address': address },
        function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                map.setCenter(results[0].geometry.location);
            }
            else {
                alert("Unable to find location");
            }
        }
    );
}


function onResize() {
  $('#map').width($(document).width()-40);
  $('#map').height($(document).height()-100);
}

$(window).resize(function() {
	onResize();
});

$(document).ready(function() {
	onResize();
    init();
});