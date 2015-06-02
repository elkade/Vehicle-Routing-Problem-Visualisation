var RouteDrawer =  function(points, id) {
    this.Points = points;
    this.CurrentIndex = 0;
    this.MaxIndex = 0;
    this.Response = null;
    this.RouteIsReady = false;
    this.Callback = null;
    this.CallbackAfterDetermine = null;
    this.Id = id;
}

RouteDrawer.prototype = {
    determineRoute: function (index) {
        var t = this;
        var subArrayPoints;
        if (index == null) {
            
            subArrayPoints = this.Points.slice(0, 8);
            t.removePoints(7);
            this.sendRequestToService(subArrayPoints, this.determineRoute, 1);
        } else {
            if (t.Points.length === 0) {
                this.RouteIsReady = true;
                if (this.CallbackAfterDetermine != null) {
                    this.CallbackAfterDetermine();
                }

                if (this.Callback != null) {
                    this.Callback();
                }
                return;
            }
            if (t.Points.length === 8) {
                subArrayPoints = this.Points.slice(0, 7);
                t.removePoints(6);
                
            } else {
                subArrayPoints = this.Points.slice(0, 8);
                t.removePoints(7);
            } 
            setTimeout(function() {
                t.sendRequestToService(subArrayPoints, t.determineRoute, index + 1);
            }, 550);
        }

    },

    removePoints: function(count) {
        for (var i = 0; i < count && this.Points.length > 0; i++) {
            this.Points.shift();
        }
    },

    draw: function() {
        if (this.RouteIsReady) {

            directionsDisplay.setDirections(this.Response);
            directionsDisplay.setMap(map);
            directionsDisplay.setPanel(document.getElementById('directions-panel'));
            $('#directions-panel').css('background-color', 'white');
            $('#directPanel').css('background-color', 'white');
            $('#directPanel').css('display', 'block');
        }
    },

    saveResponse: function (response) {
        if (this.Response == null) {
            this.Response = response;
        } else {

            var tempTable = response.routes[0].overview_path;
            for (var j = 0; j < tempTable.length; j++) {
                this.Response.routes[0].overview_path[this.Response.routes[0].overview_path.length] = tempTable[j];
            }

            var tempTable2 = response.routes[0].legs;
            for (var i = 0; i < tempTable2.length; i++) {
                this.Response.routes[0].legs[this.Response.routes[0].legs.length] = tempTable2[i];
            }

            var tempTable3 = response.routes[0].waypoint_order;
            for (var k = 0; k < tempTable3.length; k++) {
                this.Response.routes[0].waypoint_order[this.Response.routes[0].waypoint_order.length] = tempTable3[k];
            }
        }
    },

    sendRequestToService: function (pointList, callback, ind) {
        var t = this;
        var waypointsTable = [];
        for (var i = 0; i < pointList.length; i++) {
            if (i > 0 && i < pointList.length - 1) {
                waypointsTable.push({
                    location: pointList[i],
                    stopover: true
                });
            }
        }

        var request = {
            origin: pointList[0],
            destination: pointList[pointList.length - 1],
            waypoints: waypointsTable,
            travelMode: window.google.maps.TravelMode.DRIVING,
        };

        directionsService.route(request, function (response, status) {
            console.log(status);
            if (status === window.google.maps.DirectionsStatus.OK) {
                t.saveResponse(response);
                callback.call(t, ind);
            } else {
                if (this.Callback != null) {
                    this.Callback();
                } else {
                    $('#spinner').css('display', 'none');
                    alert("Nie można wyznaczyć trasy trasy");
                }
            }
        });
    }
}