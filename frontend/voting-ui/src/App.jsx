import { useEffect, useState } from 'react';
import './App.css'; // Подключим стили

function App() {
  const [polls, setPolls] = useState([]);
  const [selectedPoll, setSelectedPoll] = useState(null);
  const [answers, setAnswers] = useState({});
  const [results, setResults] = useState(null);

  useEffect(() => {
    fetch('/api/poll')
      .then(res => res.json())
      .then(setPolls);
  }, []);

  const loadPoll = async (id) => {
    const res = await fetch(`/api/poll/${id}`);
    const data = await res.json();
    setSelectedPoll(data);
    setAnswers({});
    setResults(null);
  };

  const submitVote = async () => {
    const selectedOptionIds = Object.values(answers);
    await fetch('/api/poll/vote', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(selectedOptionIds)
    });
    const res = await fetch(`/api/poll/${selectedPoll.id}/results`);
    const data = await res.json();
    setResults(data);
  };

  const goHome = () => {
    setSelectedPoll(null);
    setResults(null);
  };

  return (
    <div className="container">
      {!selectedPoll && !results && (
        <>
          <h1>Select a Survey</h1>
          <ul className="poll-list">
            {polls.map(p => (
              <li key={p.id}>
                <button className="btn" onClick={() => loadPoll(p.id)}>{p.title}</button>
              </li>
            ))}
          </ul>
        </>
      )}

      {selectedPoll && !results && (
        <>
          <h2>{selectedPoll.title}</h2>
          {selectedPoll.questions.map(q => (
            <div key={q.id} className="question">
              <p>{q.text}</p>
              <div className="options-grid">
                {q.options.map(o => (
                  <label key={o.id} className="option-card">
                    <input
                      type="radio"
                      name={`q-${q.id}`}
                      value={o.id}
                      onChange={() => setAnswers(prev => ({ ...prev, [q.id]: o.id }))}
                    />
                    {o.text}
                  </label>
                ))}
              </div>
            </div>
          ))}
          <button className="btn" onClick={submitVote}>Submit Vote</button>
        </>
      )}

      {results && (
        <>
          <h1>Results</h1>
          {results.map((q, i) => (
            <div key={i} className="question">
              <h3>{q.text}</h3>
              <ul className="results-list">
                {q.options.map((o, j) => (
                  <li key={j}>{o.text}: {o.percentage.toFixed(1)}%</li>
                ))}
              </ul>
            </div>
          ))}
          <button className="btn" onClick={goHome}>Back to Home</button>
        </>
      )}
    </div>
  );
}

export default App;
