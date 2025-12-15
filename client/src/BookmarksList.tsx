import { useState,useEffect } from "react";
import { ChevronDown, Trash2 } from "lucide-react";
import axios from "axios";
import { BACKEND_API_KEY } from "./HomePage";
import type { DeliveryAddresses } from "./models/deliveryAddressModel";



interface BookmarksListProps {
    isPanelOpen: boolean;
    onClose: () => void;
}


// date format
const options: Intl.DateTimeFormatOptions = {
  weekday: "long",
  year: "numeric",
  month: "long",
  day: "numeric",
};








function BookmarksList({
  isPanelOpen,
  onClose,
}: BookmarksListProps) {
  const [expanded, setExpanded] = useState<number | null>(null);
  const [fetchedBookmarks, setFetchedBookmarks] = useState<DeliveryAddresses[]>([]);
  const [notification, setNotification] = useState<{ type: "success" | "error"; message: string } | null>(null);


  const toggleExpand = (index: number) => {
    setExpanded(expanded === index ? null : index);
  };

  const fetchBookmarks = async () => {
   
   
    try {
      const token = sessionStorage.getItem('token');
      const response = await axios.get(`${BACKEND_API_KEY}/bookmark/`, {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        withCredentials: true,
        
      });

      if (response.status === 200) {
        setFetchedBookmarks(response.data as any);
        console.log(response.data);
      }   
      
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        // This is an expected case when there are no bookmarks
        setFetchedBookmarks([]); // Set to empty array instead of null
      } else {
        console.error("Failed to fetch bookmarks:", error);
      }
    }
  };
  
  
  useEffect(() => {
    
    fetchBookmarks();
    
  }, []);

  const handleDelete = async (id: number) => {
  try {
    const token = sessionStorage.getItem("token");

    const response = await axios.delete(`${BACKEND_API_KEY}/bookmark/${id}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      withCredentials: true,
    });

    if (response.status === 204) {
      // Update local state (remove deleted bookmark)
      setFetchedBookmarks(prev => prev.filter(b => b.id !== id));

      // Show success notification
      setNotification({ type: "success", message: "Bookmark deleted successfully" });

      // Hide after 3s
      setTimeout(() => setNotification(null), 3000);
    }
  } catch (error: any) {
    console.error("Delete failed:", error);

    setNotification({ type: "error", message: "Failed to delete bookmark" });
    setTimeout(() => setNotification(null), 3000);
  }
};





  return (
    <>
      {/* Overlay */}
      {isPanelOpen && (
        <div
          className="fixed inset-0 bg-black/30 z-40"
          onClick={onClose}
        ></div>
      )}

      {/* Sliding Panel */}
      <div
        className={`fixed top-0 right-0 h-full w-1/3 bg-white shadow-lg z-50 transform transition-transform duration-300 ${isPanelOpen ? "translate-x-0" : "translate-x-full"
          }`}
      >
        
        <div className="p-6 overflow-y-auto h-full space-y-4">
        {notification && (
          <div
            className={`p-3 rounded-md text-white ${
              notification.type === "success" ? "bg-green-500" : "bg-red-500"
            }`}
          >
            {notification.message}
          </div>
        )}
          {fetchedBookmarks && fetchedBookmarks.length > 0 ?
            (fetchedBookmarks?.map((bookmark, index) => {
              const addresses =
                (bookmark as any).deliveryaddresses ??
                (bookmark as any).deliveryAddresses ??
                [];

              if (addresses.length === 0) return null;

              const first = addresses[0];
              const last = addresses[addresses.length - 1];

              return (
                <div
  key={index}
  className="relative flex border border-gray-200 rounded-lg group hover:border-gray-400 hover:bg-gray-100 transition"
>
  {/* Left side: bookmark content */}
  <div className="flex-1 p-4">
    {/* Bookmark creation date */}
    <p className="text-gray-600 mb-2">
      {bookmark.createdAt
        ? new Date(bookmark.createdAt).toLocaleString("en-US", options)
        : "No date available"}
    </p>

    {/* First address block */}
    <div className="mb-2">
      <div className="text-lg font-medium">
        {first.street}, {first.city}
      </div>
      <div className="text-sm text-gray-600">
        Postal Code: {first.postalCode}
      </div>
    </div>

    {/* Route details */}
    <div className="mt-2">
      {expanded === index ? (
        addresses.slice(1).map((addr: any, idx: number) => (
          <div key={idx} className="mb-2">
            <div className="text-lg font-medium">
              {addr.street}, {addr.city}
            </div>
            <div className="text-sm text-gray-600">
              Postal Code: {addr.postalCode}
            </div>
          </div>
        ))
      ) : (
        <div className="mb-2">
          <div className="text-lg font-medium">
            {last.street}, {last.city}
          </div>
          <div className="text-sm text-gray-600">
            Postal Code: {last.postalCode}
          </div>
        </div>
      )}
    </div>
  </div>

  {/* Right side: full-height delete button */}
  <button
  id="deleteRoute"
  onClick={() => handleDelete(bookmark.id ?? 0)}
  className="hidden group-hover:flex items-center justify-center px-4 cursor-pointer rounded-r-lg hover:bg-red-100 transition-colors"
>
  <Trash2
    className="text-gray-400 group-hover:text-black hover:!text-red-600 transition-colors"
  />
</button>

  {/* Top-right corner: Full Route toggle */}
  {addresses.length > 2 && (
    <div className="absolute top-4 right-15">
      <button
        onClick={() => toggleExpand(index)}
        className="flex items-center text-blue-600 text-sm"
      >
        {expanded === index ? "Hide Route" : "Full Route"}
        <ChevronDown
          className={`ml-1 w-4 h-4 transform transition-transform ${
            expanded === index ? "rotate-180" : ""
          }`}
        />
      </button>
    </div>
  )}
</div>


              );
            })) : (<p className="flex text-gray-600 font-bold font-size-30 justify-center ">No Saved Routes</p>)}
        </div>
      </div>
    </>
  );
}

export default BookmarksList;