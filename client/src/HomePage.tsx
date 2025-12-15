import { useEffect, useState } from "react";
import axios from "axios";
import type { BookmarkResponse, DeliveryAddresses, DeliveryAddressResponse, PriorityLevel } from "./models/deliveryAddressModel";
import { CheckCircle, XCircle, Bookmark, ChevronLeft, LogOutIcon } from "lucide-react";
import BookmarksList from "./BookmarksList";
import { useNavigate } from "react-router-dom";
import { logout } from "./api";


export const BACKEND_API_KEY = 'http://localhost:5000/api';

function HomePage() {
  const [route, setRoute] = useState<string>("");
  const [checked, setChecked] = useState<boolean>(false);
  const [isGenerating, setIsGenerating] = useState<boolean>(false);
  const [result, setResult] = useState<DeliveryAddresses[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<boolean>(false);
  const [message, setMessage] = useState<string>("");
  const [routeCount, setRouteCount] = useState<number>(0);
  const [bookmarkBg, setBookmarkBg] = useState("bg-white");
  const [date, setDate] = useState<string>('');
  const [isPanelOpen, setIsPanelOpen] = useState<boolean>(false);

  const [logoutMessage, setLogoutMessage] = useState<string> ('');
  const [successLogout, setSuccessLogout] = useState<boolean> (false);

  const navigate = useNavigate();

  useEffect(() => {
    // Check if user is authenticated
    const token = sessionStorage.getItem('token');
    if (!token) {
      navigate('/login');
    }
  }, [navigate]);

  const handleLogout = async () => {
    const result = await logout();
    console.log(result);
    if (result){
      setSuccessLogout(true); 
      setLogoutMessage(result.message);
    setTimeout(() => navigate('/login'), 3000);
    
    }
    else {
      setSuccessLogout(false); 
      setLogoutMessage(result.message);
      setTimeout(() => 3000);
    }
  };

  const handleBookmark = async () => {
    if (!result) return;

    const data = await BookmarkRoute(result);
    if (data) {
      setBookmarkBg(data.bookmark?.length ? "bg-yellow-400" : "bg-white");
    } else {
      setBookmarkBg("bg-white");
    }
  };

  const BookmarkRoute = async (saveroute: DeliveryAddresses[]): Promise<BookmarkResponse | undefined> => {
    try {
      console.log(saveroute);
      const bookmarkPayload = [
        {
          
          deliveryAddresses: saveroute.map(addr => ({
            street: addr.street,
            city: addr.city,
            postalCode: addr.postalCode,
            priority: addr.priority,
            geolocationId: addr.geolocationId,
            geoLocation: {
              latitude: addr.geolocation?.latitude ?? 0,
              longitude: addr.geolocation?.longitude ?? 0
            },
            createdAt: date
          }))
        }
      ];
      const token = sessionStorage.getItem('token');
        const response = await axios.post(
            `${BACKEND_API_KEY}/bookmark`, 
            bookmarkPayload,
            {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            }
        );

      if (response.status === 201) {
        setIsPanelOpen(true);
        setSuccess(true);
        setMessage(response.data.message);
        setTimeout(() => { setSuccess(false) }, 3000);
        return response.data;
      }
    } catch (error) {
      setSuccess(false);
      setError("Failed to bookmark route");
      setTimeout(() => { setError(null) }, 3000)
    }
    return undefined;
  };

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setIsGenerating(true);
    setError(null);


    // Determine if this is the start route (first address)
    const isStartRoute = routeCount === 0;
    const currentPriority: PriorityLevel = isStartRoute ? 0 : (checked ? 1 : 2);

    try {
      // Validate input
      if (!route.trim()) {
        setError("Please enter an address");
        return;
      }
              
      const token = sessionStorage.getItem('token');
      const response = await axios.post<DeliveryAddressResponse>(
        `${BACKEND_API_KEY}/route`,
        {
          address: route.trim(), // Make sure to match the backend DTO property name
          priority: currentPriority
           
          },
          {
            headers: {
              'Authorization':`Bearer ${token}` 
            }
          }
        
      );

      if (Array.isArray(response.data)) {
        // Case: backend returned just an array
        setResult(response.data);
        setSuccess(true)
        setMessage("Route optimized successfully");
      } else {
        // Case: backend returned object with Message
        setResult(response.data.deliveryAddresses ?? null);
        setSuccess(true);
        setMessage(response.data.message);
      }
      setDate(new Date().toISOString());


      setTimeout(() => { setSuccess(false) }, 3000);
      setRoute("");  // Clear input after success 
      setRouteCount(prev => prev + 1); // Increment route count after successful addition
    } catch (err) {
      if (axios.isAxiosError(err)) {
        // Handles specific error cases
        if (err.response?.status === 404) {
          setError("Address not found. Please check the address and try again.");
        } else if (err.response?.status === 400) {
          setError("Invalid address format. Please try again.");
        } else {
          setError("Failed to process route. Please try again later.");
        }
        console.error("API Error:", err.response?.data);
      } else {
        setError("An unexpected error occurred. Please try again.");
        console.error("Unknown Error:", err);
      }
    } finally {
      setIsGenerating(false);
    }
  }



  return (
    <div id="homePageContent" className="flex relative flex-col min-h-screen justify-center items-center bg-gradient-to-b from-gray-50 to-gray-200 p-6 gap-8">
      {logoutMessage && (successLogout ? (
        <div className="flex items-center gap-2 p-4 rounded-md bg-green-100 border border-green-400 text-green-700 text-sm">
          <CheckCircle className="w-5 h-5 text-green-600" />
          <span>{message}</span>
        </div>
      ) : (
        <div className="flex items-center gap-2 p-4 rounded-md bg-red-100 border border-red-400 text-red-700 text-sm">
          <XCircle className="w-5 h-5 text-red-600" />
          <span>{message}</span>
        </div>))}
      <button id="logoutButton" onClick={() => handleLogout()}
      className="absolute right-10 top-10 group flex items-center space-x-2 hover:text-red-600 transition-colors duration-300">
  <LogOutIcon className="w-6 h-6" />
  <span className="opacity-0 group-hover:opacity-100 transition-opacity duration-300 text-sm">
    Log out
  </span>
</button>
      {/* Success Message */}
      {success && (
        <div className="flex border-gray-600 justify-content items-center border-black-300 mb-4 gap-2">
          <CheckCircle className="text-green-800" size={20} /><span id="success-msg" className="text-green-600 text-align font-semibold">{message}</span>
        </div>
      )}
      {/* Logo */}
      <img
        className="w-48 h-auto"
        src="/src/assets/RoadWiseLogo.png"
        alt="RouteWise Logo"
      />
      <>
      <button
      id="openList"
  onClick={() => setIsPanelOpen(prev => !prev)}
  className="fixed right-0 top-1/2 -translate-y-1/2 flex items-center gap-2 bg-black hover:bg-gray-800 text-white px-6 py-3 rounded-l-lg transform -rotate-90 translate-x-8 transition-all duration-300 shadow-lg z-50"
>
  <ChevronLeft className="w-5 h-5" />
  <span className="font-medium">Saved Routes</span>
</button>
  {/* Modal Button */}
  {isPanelOpen && (
  <BookmarksList
    isPanelOpen={isPanelOpen} 
    onClose={() => setIsPanelOpen(false)}

  />
)}
</>

      {/* Input Form */}
      <form
        onSubmit={handleSubmit}
        className="flex w-full max-w-3xl items-center gap-4 bg-white border border-gray-200 rounded-xl shadow-md p-5"
      >
        <input
        id="addressInput"
          value={route}
          onChange={(e) => setRoute(e.target.value)}
          className="flex-grow bg-transparent outline-none text-gray-900 placeholder-gray-500 p-3 text-lg"
          type="text"
          placeholder={routeCount === 0 ? "Enter your starting point (e.g., Street, Postcode)" : "Enter destination address..."}
        />

        <label className="flex items-center gap-2 text-gray-700 text-sm">
          <input
            id="priorityCheckbox"
            checked={checked}
            onChange={(e) => setChecked(e.target.checked)}
            type="checkbox"
            className="w-5 h-5 accent-black"
          />
          High Priority
        </label>

        <button
        id="submitAddress"
          type="submit"
          disabled={isGenerating}
          className="bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 transition text-lg disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isGenerating ? "Processing..." : "Submit"}
        </button>
      </form>

      {/* Results Section */}
      <div id="routeList" className="w-full max-w-3xl">
        {isGenerating && (
          <div className="flex items-center justify-center gap-3 text-gray-600 text-lg animate-pulse">
            <span className="loader w-5 h-5 border-2 border-gray-300 border-t-black rounded-full animate-spin"></span>
            Generating your optimized route...
          </div>
        )}

        {error && (<div id="error-msg" className="flex justify-center items-center gap-2">
          <XCircle className="text-red-800 " size={20} /> <span className="text-red-600 font-medium">{error}</span>
        </div>

        )}

        {Array.isArray(result) && !isGenerating && (
          <div className="mt-6 bg-white border border-gray-200 rounded-xl shadow p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-800">
                Optimized Route
              </h2>
              <button id="addBookmark" onClick={handleBookmark} className="hover:text-gray-600 transition-colors overflow-hidden">
                <Bookmark className={`text-yellow-200 ${bookmarkBg}`} size={24} />
              </button>
            </div>
            <ul className="space-y-3">
              {result.map((addr, index) => (
                <div key={index}>
                  <li className="p-4 border border-gray-100 rounded-md hover:bg-gray-50 transition">
                    <p className="text-lg font-medium text-gray-900">
                      {addr.street}, {addr.city}
                    </p>
                    <p className="text-sm text-gray-600">
                      Postal: {addr.postalCode} — Priority:{" "}
                      <span
                        className={`font-semibold ${index === result.length - 1
                          ? "text-red-600"
                          : addr.priority === 0
                            ? "text-green-600"
                            : addr.priority === 1
                              ? "text-orange-600"
                              : "text-gray-500"
                          }`}
                      >
                        {index === result.length - 1 ? "Stop"
                          : addr.priority === 0 ? "Start"
                            : addr.priority === 1 ? "High" : "Normal"}
                      </span>
                    </p>
                  </li>
                  {index < result.length - 1 && (
                    <div className="flex justify-center items-center rotate-90 py-4">
                      <div className="text-gray-500 font-bold text-2xl">•</div>
                      <div className="text-gray-500 font-bold text-2xl">•</div>
                    </div>
                  )}
                </div>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}

export default HomePage;